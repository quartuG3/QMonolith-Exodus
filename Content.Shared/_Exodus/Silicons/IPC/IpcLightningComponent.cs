using Content.Shared.Actions;
using Content.Shared.Damage;
using Content.Shared.FixedPoint;

namespace Content.Shared._Exodus.Silicons.IPC;

[RegisterComponent]
public sealed partial class IpcLightningComponent : Component
{
    [DataField]
    public float ChargeFractionCost = 0.5f;

    [DataField]
    public DamageSpecifier DamageAtFullCharge = new()
    {
        DamageDict = new()
        {
            { "Shock", FixedPoint2.New(35) },
        },
    };

    [DataField]
    public string LightningPrototype = "Lightning";
}

public sealed partial class IpcLightningActionEvent : EntityTargetActionEvent;
