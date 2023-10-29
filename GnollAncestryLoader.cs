using System.Collections.Generic;
using System.Linq;
using Dawnsbury.Audio;
using Dawnsbury.Auxiliary;
using Dawnsbury.Core.Animations;
using Dawnsbury.Core.CharacterBuilder.AbilityScores;
using Dawnsbury.Core.CharacterBuilder.Feats;
using Dawnsbury.Core.CharacterBuilder.FeatsDb.Common;
using Dawnsbury.Core.CharacterBuilder.Selections.Options;
using Dawnsbury.Core.CombatActions;
using Dawnsbury.Core.Creatures;
using Dawnsbury.Core.Mechanics;
using Dawnsbury.Core.Mechanics.Core;
using Dawnsbury.Core.Mechanics.Enumerations;
using Dawnsbury.Core.Mechanics.Targeting;
using Dawnsbury.Core.Mechanics.Treasure;
using Dawnsbury.Core.Possibilities;
using Dawnsbury.Modding;
using Dawnsbury.Core;

#pragma warning disable CS1998 // Async method lacks 'await' operators and will run synchronously

namespace Ancestries.Gnoll;

public static class GnollAncestryLoader
{
    public static Trait GnollTrait;

    [DawnsburyDaysModMainMethod]
    public static void LoadMod()
    {
        GnollTrait = ModManager.RegisterTrait(
            "Gnoll",
            new TraitProperties("Gnoll", true)
            {
                IsAncestryTrait = true
            });
        AddFeats(CreateGnollAncestryFeats());

        ModManager.AddFeat(new AncestrySelectionFeat(
                FeatName.CustomFeat,
                "Powerfully-built humanoids that resemble hyenas, gnolls are cunning warriors and hunters. Their frightening visage and efficient tactics have given them an ill-starred reputation.",
                new List<Trait> { Trait.Humanoid, GnollTrait },
                8,
                5,
                new List<AbilityBoost>()
                {
                    new EnforcedAbilityBoost(Ability.Strength),
                    new EnforcedAbilityBoost(Ability.Intelligence),
                    new FreeAbilityBoost()
                },
                CreateGnollHeritages().ToList())
            .WithAbilityFlaw(Ability.Wisdom)
            .WithCustomName("Gnoll")
            .WithOnCreature(creature =>
            {
                creature.AddQEffect(new QEffect("Bite", "You have a jaws attack.")
                {
                    AdditionalUnarmedStrike = new Item(IllustrationName.Jaws, "jaws",
                            new[] { Trait.Unarmed, Trait.Melee, Trait.Weapon })
                        .WithWeaponProperties(new WeaponProperties("1d6", DamageKind.Piercing))
                });
            })
        );
    }

    private static void AddFeats(IEnumerable<Feat> feats)
    {
        foreach (var feat in feats)
        {
            ModManager.AddFeat(feat);
        }
    }

    private static IEnumerable<Feat> CreateGnollAncestryFeats()
    {
        yield return new GnollAncestryFeat(
                "Crunch",
                "Your jaws can crush bone and bite through armor.",
                "Your jaws unarmed attack deals 1d8 piercing damage instead of 1d6 and gains the grapple trait.")
            .WithCustomName("Crunch")
            .WithOnCreature(creature =>
            {
                creature.QEffects.First(qe => qe.Name == "Bite").AdditionalUnarmedStrike =
                    new Item(IllustrationName.Jaws, "jaws",
                    new[] { Trait.Unarmed, Trait.Melee, Trait.Weapon, Trait.Grab })
                    .WithWeaponProperties(new WeaponProperties("1d8", DamageKind.Piercing));
            });

        yield return new GnollAncestryFeat("Gnoll Weapon Familiarity",
                "You were taught to be a hunter and a raider.",
                "You are trained with flails, khopeshes, mambeles, spears, and war flails.")
            .WithOnSheet(sheet =>
            {
                sheet.SetProficiency(Trait.Flail, Proficiency.Trained);
                sheet.SetProficiency(Trait.Spear, Proficiency.Trained);
            });
    }

     private static IEnumerable<Feat> CreateGnollHeritages()
    {
        yield return new HeritageSelectionFeat(FeatName.CustomFeat,
                "You’re a large, powerful gnoll, with tawny fur and brown spots on your hide.",
                "You gain 10 Hit Points from your ancestry instead of 8 and gain a +1 circumstance bonus to Athletics checks to Shove or Trip foes.")
            .WithCustomName("Great Gnoll")
            .WithOnCreature(creature => creature.AddQEffect(new QEffect("Great Gnoll", "You have a +1 circumstance bonus to Athletics checks to Shove or Trip foes.")
            {
                Innate = true,
                BonusToSkillChecks = (Skill skill, CombatAction action, Creature creature) => (action != null && skill == Skill.Athletics && (action.Traits.Contains(Trait.Shove) || action.Traits.Contains(Trait.Trip))) ? new Bonus(1, BonusType.Circumstance, "Great Gnoll") : null
            }));
     

    }
}