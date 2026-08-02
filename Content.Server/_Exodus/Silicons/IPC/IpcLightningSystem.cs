using System.Diagnostics.CodeAnalysis;
using Content.Server.Lightning;
using Content.Server.Power.EntitySystems;
using Content.Server.PowerCell;
using Content.Server.Popups;
using Content.Shared._Exodus.Silicons.IPC;
using Content.Shared.Damage;
using Content.Shared.Power.Components;
using Content.Shared.PowerCell.Components;
using Content.Shared.Popups;

namespace Content.Server._Exodus.Silicons.IPC;

public sealed class IpcLightningSystem : EntitySystem
{
    [Dependency] private readonly BatterySystem _battery = default!;
    [Dependency] private readonly DamageableSystem _damageable = default!;
    [Dependency] private readonly LightningSystem _lightning = default!;
    [Dependency] private readonly PopupSystem _popup = default!;
    [Dependency] private readonly PowerCellSystem _powerCell = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<IpcLightningComponent, IpcLightningActionEvent>(OnLightningAction);
    }

    private void OnLightningAction(Entity<IpcLightningComponent> ent, ref IpcLightningActionEvent args)
    {
        if (args.Handled || args.Target == ent.Owner)
            return;

        if (!TryGetBattery(ent.Owner, out var batteryUid, out var battery))
        {
            _popup.PopupEntity(Loc.GetString("ipc-lightning-no-battery"), ent.Owner, ent.Owner, PopupType.SmallCaution);
            return;
        }

        if (battery.MaxCharge <= 0f)
            return;

        var cost = battery.MaxCharge * ent.Comp.ChargeFractionCost;
        if (battery.CurrentCharge < cost)
        {
            _popup.PopupEntity(Loc.GetString("ipc-lightning-not-enough-charge"), ent.Owner, ent.Owner, PopupType.SmallCaution);
            return;
        }

        var chargeRatio = Math.Clamp(battery.CurrentCharge / battery.MaxCharge, 0f, 1f);
        var damage = ent.Comp.DamageAtFullCharge * chargeRatio;

        if (!_battery.TryUseCharge(batteryUid, cost, battery))
            return;

        _lightning.ShootLightning(ent.Owner, args.Target, ent.Comp.LightningPrototype);
        _damageable.TryChangeDamage(args.Target, damage, true, false, origin: ent.Owner);

        args.Handled = true;
    }

    private bool TryGetBattery(EntityUid uid, out EntityUid batteryUid, [NotNullWhen(true)] out BatteryComponent? battery)
    {
        if (TryComp(uid, out battery))
        {
            batteryUid = uid;
            return true;
        }

        if (_powerCell.TryGetBatteryFromSlot(uid, out var batteryEnt, out battery, CompOrNull<PowerCellSlotComponent>(uid)) && batteryEnt != null)
        {
            batteryUid = batteryEnt.Value;
            return true;
        }

        batteryUid = EntityUid.Invalid;
        battery = null;
        return false;
    }
}
