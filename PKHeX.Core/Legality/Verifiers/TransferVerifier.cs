﻿using System;
using System.Collections.Generic;
using static PKHeX.Core.LegalityCheckStrings;

namespace PKHeX.Core
{
    /// <summary>
    /// Verifies the transfer data for a <see cref="PKM"/> that has been irreversably transferred forward.
    /// </summary>
    public sealed class TransferVerifier : Verifier
    {
        protected override CheckIdentifier Identifier => CheckIdentifier.Encounter;

        public override void Verify(LegalityAnalysis data)
        {
            throw new Exception("Don't call via this.");
        }

        public void VerifyTransferLegalityG12(LegalityAnalysis data)
        {
            VerifyTransferVCNatureEXP(data);
        }

        private void VerifyTransferVCNatureEXP(LegalityAnalysis data)
        {
            var pkm = data.pkm;
            var met = pkm.Met_Level;

            if (met == 100) // check for precise match, can't receive EXP after transfer.
            {
                var nature = Experience.GetNatureVC(pkm.EXP);
                if (nature != pkm.Nature)
                    data.AddLine(GetInvalid(LTransferNature));
                return;
            }
            if (met <= 2) // Not enough EXP to have every nature -- check for exclusions!
            {
                var pi = pkm.PersonalInfo;
                var growth = pi.EXPGrowth;
                var nature = pkm.Nature;
                bool valid = VerifyVCNature(growth, nature);
                if (!valid)
                    data.AddLine(GetInvalid(LTransferNature));
            }
        }

        private static bool VerifyVCNature(int growth, int nature)
        {
            // exp % 25 with a limited amount of EXP does not allow for every nature
            return growth switch
            {
                0 => // MediumFast -- Can't be Brave, Adamant, Naughty, Bold, Docile, or Relaxed
                (nature < (int) Nature.Brave || nature > (int) Nature.Relaxed),
                4 => // Fast -- Can't be Gentle, Sassy, Careful, Quirky, Hardy, Lonely, Brave, Adamant, Naughty, or Bold
                (nature < (int) Nature.Gentle && nature > (int) Nature.Bold),
                5 => // Slow -- Can't be Impish or Lax
                (nature != (int) Nature.Impish && nature != (int) Nature.Lax),
                _ => true
            };
        }

        public void VerifyTransferLegalityG3(LegalityAnalysis data)
        {
            var pkm = data.pkm;
            if (pkm.Format == 4) // Pal Park (3->4)
            {
                if (pkm.Met_Location != Locations.Transfer3)
                    data.AddLine(GetInvalid(LEggLocationPalPark));
            }
            else // Transporter (4->5)
            {
                if (pkm.Met_Location != Locations.Transfer4)
                    data.AddLine(GetInvalid(LTransferEggLocationTransporter));
            }
        }

        public void VerifyTransferLegalityG4(LegalityAnalysis data)
        {
            var pkm = data.pkm;
            int loc = pkm.Met_Location;
            if (loc == Locations.Transfer4)
                return;

            // Crown met location must be present if transferred via lock capsule
            switch (pkm.Species)
            {
                case (int)Species.Celebi:
                    if (loc != Locations.Transfer4_CelebiUnused && loc != Locations.Transfer4_CelebiUsed)
                        data.AddLine(GetInvalid(LTransferMet));
                    break;
                case (int)Species.Raikou:
                case (int)Species.Entei:
                case (int)Species.Suicune:
                    if (loc != Locations.Transfer4_CrownUnused && loc != Locations.Transfer4_CrownUsed)
                        data.AddLine(GetInvalid(LTransferMet));
                    break;
                default:
                    data.AddLine(GetInvalid(LTransferEggLocationTransporter));
                    break;
            }
        }

        public void VerifyTransferLegalityG8(LegalityAnalysis data)
        {
            var pkm = data.pkm;
            int species = pkm.Species;
            var pi = (PersonalInfoSWSH)PersonalTable.SWSH.GetFormeEntry(species, pkm.AltForm);
            if (!pi.IsPresentInGame) // Can't transfer
                data.AddLine(GetInvalid(LTransferBad));
            else if (pkm.GenNumber != 8) // Can't transfer yet
                data.AddLine(GetInvalid(LTransferBad));
            else if (pi.PokeDexIndex == 0 && data.EncounterMatch is EncounterEgg) // Can't breed cuz no transfer yet
                data.AddLine(GetInvalid(LTransferBad));
            else if (IsG8BanForm(pkm))
                data.AddLine(GetInvalid(LTransferBad));
        }

        private static bool IsG8BanForm(PKM pkm)
        {
            return pkm.Species switch
            {
                (int)Species.Raichu     when pkm.AltForm == 1 => true,
                (int)Species.Vulpix     when pkm.AltForm == 1 => true,
                (int)Species.Ninetales  when pkm.AltForm == 1 => true,
                (int)Species.Diglett    when pkm.AltForm == 1 => true,
                (int)Species.Dugtrio    when pkm.AltForm == 1 => true,
                (int)Species.Meowth     when pkm.AltForm == 1 => true,
                (int)Species.Persian    when pkm.AltForm == 1 => true,
                (int)Species.Ponyta     when pkm.AltForm == 0 => true,
                (int)Species.Rapidash   when pkm.AltForm == 0 => true,
                (int)Species.Farfetchd  when pkm.AltForm == 0 => true,
                (int)Species.Weezing    when pkm.AltForm == 0 => true,
                (int)Species.Corsola    when pkm.AltForm == 0 => true,
                (int)Species.Zigzagoon  when pkm.AltForm == 0 => true,
                (int)Species.Linoone    when pkm.AltForm == 0 => true,
                (int)Species.Shellos    when pkm.AltForm == 0 => true,
                (int)Species.Gastrodon  when pkm.AltForm == 0 => true,
                (int)Species.Darumaka   when pkm.AltForm == 0 => true,
                (int)Species.Darmanitan when pkm.AltForm == 0 => true,
                (int)Species.Stunfisk   when pkm.AltForm == 0 => true,
                _ => false
            };
        }

        public IEnumerable<CheckResult> VerifyVCEncounter(PKM pkm, IEncounterable encounter, ILocation transfer, IList<CheckMoveResult> Moves)
        {
            // Check existing EncounterMatch
            if (encounter is EncounterInvalid || transfer == null)
                yield break; // Avoid duplicate invalid message

            if (encounter is EncounterStatic v && (GameVersion.GBCartEraOnly.Contains(v.Version) || v.Version == GameVersion.VCEvents))
            {
                bool exceptions = false;
                exceptions |= v.Version == GameVersion.VCEvents && encounter.Species == (int)Species.Mew && pkm.TID == 22796;
                if (!exceptions)
                    yield return GetInvalid(LG1GBEncounter);
            }

            if (pkm.Met_Location != transfer.Location)
                yield return GetInvalid(LTransferMetLocation);
            if (pkm.Egg_Location != transfer.EggLocation)
                yield return GetInvalid(LEggLocationNone);

            // Flag Moves that cannot be transferred
            if (encounter is EncounterStatic s && s.Version == GameVersion.C && s.EggLocation == 256) // Dizzy Punch Gifts
                FlagIncompatibleTransferMove(pkm, Moves, 146, 2); // can't have Dizzy Punch at all

            bool checkShiny = pkm.VC2 || (pkm.TradebackStatus == TradebackType.WasTradeback && pkm.VC1);
            if (!checkShiny)
                yield break;

            if (pkm.Gender == 1) // female
            {
                if (pkm.PersonalInfo.Gender == 31 && pkm.IsShiny) // impossible gender-shiny
                    yield return GetInvalid(LEncStaticPIDShiny, CheckIdentifier.PID);
            }
            else if (pkm.Species == (int)Species.Unown)
            {
                if (pkm.AltForm != 8 && pkm.AltForm != 21 && pkm.IsShiny) // impossibly form-shiny (not I or V)
                    yield return GetInvalid(LEncStaticPIDShiny, CheckIdentifier.PID);
            }
        }

        private static void FlagIncompatibleTransferMove(PKM pkm, IList<CheckMoveResult> Moves, int move, int gen)
        {
            int index = Array.IndexOf(pkm.Moves, move);
            if (index < 0)
                return; // doesn't have move

            var chk = Moves[index];
            if (chk.Generation == gen) // not obtained from a future gen
                Moves[index] = new CheckMoveResult(chk.Source, chk.Generation, Severity.Invalid, LTransferMove, CheckIdentifier.Move);
        }
    }
}
