using Content.Server.Botany.Components;
using Content.Server.Botany.Systems;
using Content.Shared.EntityEffects;
using JetBrains.Annotations;
using Robust.Shared.Prototypes;
using Robust.Shared.Random;
using Content.Shared.Chemistry.EntitySystems;
using Content.Shared.FixedPoint;
using Content.Shared.Chemistry.Reagent;
using Content.Server.Botany;
namespace Content.Server.EntityEffects.Effects.PlantMetabolism;

[UsedImplicitly]
[DataDefinition]
public sealed partial class PlantChloroforge : EntityEffect
{
    [DataField("ignoreList")]
    public List<string>? IgnoreList;

    public override void Effect(EntityEffectBaseArgs args)
    {
        if (!args.EntityManager.TryGetComponent(args.TargetEntity, out PlantHolderComponent? plantHolderComp)
                                || plantHolderComp.Seed == null || plantHolderComp.Dead ||
                                plantHolderComp.Seed.Immutable)
            return;


        var prototype = IoCManager.Resolve<IPrototypeManager>();
        var solutionContainerSystem = args.EntityManager.System<SharedSolutionContainerSystem>();
        if (!solutionContainerSystem.ResolveSolution(args.TargetEntity, plantHolderComp.SoilSolutionName, ref plantHolderComp.SoilSolution, out var solution))
            return;

        if (solution.Volume > 0)
        {
            var amt = FixedPoint2.New(1);
            foreach (var entry in solutionContainerSystem.RemoveEachReagent(plantHolderComp.SoilSolution.Value, amt))
            {
                var reagentProto = prototype.Index<ReagentPrototype>(entry.Reagent.Prototype);
                var chemicalId = reagentProto.ID;

                if (IgnoreList != null && IgnoreList.Contains(chemicalId))
                {
                    continue;
                }
                var seedChemQuantity = new SeedChemQuantity();
                if (plantHolderComp.Seed.Chemicals.ContainsKey(chemicalId))
                {
                    seedChemQuantity.Min = plantHolderComp.Seed.Chemicals[chemicalId].Min;
                    seedChemQuantity.Max = plantHolderComp.Seed.Chemicals[chemicalId].Max + 1;
                }
                else
                {
                    seedChemQuantity.Min = 1;
                    seedChemQuantity.Max = 1 + 1;
                    seedChemQuantity.Inherent = false;
                }
                var potencyDivisor = (int)Math.Ceiling(100.0f / seedChemQuantity.Max);
                seedChemQuantity.PotencyDivisor = potencyDivisor;
                plantHolderComp.Seed.Chemicals.TryAdd(chemicalId, seedChemQuantity);
            }
        }
    }

    protected override string? ReagentEffectGuidebookText(IPrototypeManager prototype, IEntitySystemManager entSys) => Loc.GetString("reagent-effect-guidebook-plant-chloroforge", ("chance", Probability));
}
