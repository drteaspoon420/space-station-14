using Robust.Shared.Audio;
using Robust.Shared.GameStates;
using Robust.Shared.Serialization;

namespace Content.Shared.Components;

[RegisterComponent, NetworkedComponent]
public sealed partial class DnaComponent : Component
{
    [DataField("sequence")]
    public string Sequence;

    private bool _sequenced = false;
    private bool _dirty = false;
}
