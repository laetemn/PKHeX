﻿using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using PKHeX.Core;

namespace PKHeX.Tests.Legality
{
    [TestClass]
    public class ShadowTests
    {
        private const string LegalityValidCategory = "Shadow Lock Validity Tests";
        private const string VerifyPIDCategory = "Shadow Lock Result Tests";

        [TestMethod]
        [TestCategory(LegalityValidCategory)]
        public void VerifyLock1()
        {
            // Zubat (F) (Serious)
            Verify(Encounters3Teams.Poochyena, 0xAF4E3161, new[] { 11, 29, 25, 6, 23, 10 });

            // Murkrow (M) (Docile)
            Verify(Encounters3Teams.Pineco, 0xC3A0F1E5, new[] { 30, 3, 9, 10, 27, 30 });
        }

        [TestMethod]
        [TestCategory(LegalityValidCategory)]
        public void VerifyLock2()
        {
            // Goldeen (F) (Serious)
            // Horsea (M) (Quirky)
            Verify(Encounters3Teams.Spheal, 0xA459BF44, new[] { 0, 11, 4, 28, 6, 13 });

            // Kirlia (M) (Hardy)
            // Linoone (F) (Hardy)
            Verify(Encounters3Teams.Natu, 0x8E14DAB6, new[] { 29, 24, 30, 16, 3, 18 });

            // Remoraid (M) (Docile) -- 73DB58CC
            // Golbat (M) (Bashful) -- F6B04390
            Verify(Encounters3Teams.Roselia, 0x30E87CC7, new[] { 22, 11, 8, 26, 4, 29 });

            // 519AEF0E
            // Duskull (M) (Quirky) -- 45BE3B97
            // Spinarak (F) (Hardy) -- E18F5A3E
            Verify(Encounters3Teams.ColoMakuhita, 0xC252FEBA, new[] { 15, 9, 17, 16, 24, 22 });

            // 559C5F72 -- Quirky F => skip
            // Duskull (M) (Quirky) -- A5AC2CCB
            // Spinarak (F) (Hardy) -- D08FF135
            Verify(Encounters3Teams.ColoMakuhita, 0x61C676FC, new[] { 20, 28, 21, 18, 9, 1 });

            // 3CCB97BA -- Quirky F => skip * 2, Hardy Skip
            // Duskull (M) (Quirky) -- 7F0D6783 @ 161
            // Spinarak (F) (Hardy) -- 6C03F545 @ 182
            Verify(Encounters3Teams.ColoMakuhita, 0x3B27608D, new[] { 7, 12, 5, 19, 3, 7 });
        }

        [TestMethod]
        [TestCategory(LegalityValidCategory)]
        public void VerifyLock3()
        {
            // Luvdisc (F) (Docile)
            // Beautifly (M) (Hardy)
            // Roselia (M) (Quirky)
            Verify(Encounters3Teams.Delcatty, 0x9BECA2A6, new[] { 31, 31, 25, 13, 22, 1 });

            // Kadabra (M) (Docile)
            // Sneasel (F) (Hardy)
            // Misdreavus (F) (Bashful)
            Verify(Encounters3Teams.Meowth, 0x77D87601, new[] { 10, 27, 26, 13, 30, 19 });

            // Ralts (M) (Docile)
            // Voltorb (-) (Hardy)
            // Bagon (F) (Quirky)
            Verify(Encounters3Teams.Numel, 0x37F95B26, new[] { 11, 8, 5, 10, 28, 14 });
        }

        [TestMethod]
        [TestCategory(LegalityValidCategory)]
        public void VerifyLock4()
        {
            // Ninetales (M) (Serious)
            // Jumpluff (M) (Docile)
            // Azumarill (F) (Hardy)
            // Shadow Tangela
            VerifySingle(Encounters3Teams.Butterfree, 0x2E49AC34, new[] { 15, 24, 7, 2, 11, 2 });

            // Huntail (M) (Docile)
            // Cacturne (F) (Hardy)
            // Weezing (F) (Serious)
            // Ursaring (F) (Bashful)
            Verify(Encounters3Teams.Arbok, 0x1973FD07, new[] { 13, 30, 3, 16, 20, 9 });

            // Lairon (F) (Bashful)
            // Sealeo (F) (Serious)
            // Slowking (F) (Docile)
            // Ursaring (M) (Quirky)
            Verify(Encounters3Teams.Primeape, 0x33893D4C, new[] { 26, 25, 24, 28, 29, 30 });
        }

        [TestMethod]
        [TestCategory(LegalityValidCategory)]
        public void VerifyLock5()
        {
            // many prior, all non shadow
            VerifySingle(Encounters3Teams.Seedot, 0x8CBD29DB, new[] { 19, 29, 30, 0, 7, 2 });
        }

        private static void Verify(TeamLock[] teams, uint pid, int[] ivs, bool xd = true)
        {
            var pk3 = new PK3 { PID = pid, IVs = ivs };
            var info = MethodFinder.Analyze(pk3);
            Assert.AreEqual(PIDType.CXD, info.Type, "Unable to match PID to CXD spread!");
            bool match = GetCanOriginateFrom(teams, info, xd, out var _);
            Assert.IsTrue(match, "Unable to verify lock conditions: " + teams[0].Species);
        }

        private static void VerifySingle(TeamLock[] teams, uint pid, int[] ivs, bool xd = true)
        {
            var pk3 = new PK3 { PID = pid, IVs = ivs };
            var info = MethodFinder.Analyze(pk3);
            Assert.AreEqual(PIDType.CXD, info.Type, "Unable to match PID to CXD spread!");
            bool match = LockFinder.IsFirstShadowLockValid(info, teams, xd);
            Assert.IsTrue(match, "Unable to verify lock conditions: " + teams[0].Species);
        }


        [TestMethod]
        [TestCategory(VerifyPIDCategory)]
        public void VerifyPIDResults()
        {
            var results = new[]
            {
                new uint[] {0xD118BA52, 0xA3127782, 0x16D95FA5, 0x31538B48},
                new uint[] {0x7D5FFE3E, 0x1D5720ED, 0xE0D89C99, 0x3494CDA1},
                new uint[] {0xAEB0C3A6, 0x956DC2FD, 0x3C11DCE8, 0xC93DF897},
                new uint[] {0xACCE2655, 0xFF2BA0A2, 0x22A8A7E6, 0x5F5380F4},
                new uint[] {0xDC1D1894, 0xFC0F75E2, 0x97BFAEBC, 0x38DDE117},
                new uint[] {0xDE278967, 0xFD86C9F7, 0x3E16FCFD, 0x1956D8B5},
                new uint[] {0xF8CB4CAE, 0x42DE628B, 0x48796CDA, 0xF6EAD3E2},
                new uint[] {0x56548F49, 0xA308E7DA, 0x28CB8ADF, 0xBEADBDC3},
                new uint[] {0xF2AC8419, 0xADA208E3, 0xDB3A0BA6, 0x5EEF1076},
                new uint[] {0x9D28899D, 0xA3ECC9F0, 0x606EC6F0, 0x451FAE3C},
            };
            VerifyResults(results, Encounters3Teams.Delcatty);
        }

        private static void VerifyResults(IReadOnlyList<uint[]> results, TeamLock[] team)
        {
            var pkm = new PK3();
            for (int i = 0; i < results.Count; i++)
            {
                var result = results[i];
                var seeds = getSeeds(result[result.Length - 1]);
                bool match = false;
                foreach (var seed in seeds)
                {
                    PIDGenerator.SetValuesFromSeed(pkm, PIDType.CXD, seed);
                    var info = MethodFinder.Analyze(pkm);
                    Assert.IsTrue(seed == info.OriginSeed);
                    Assert.AreEqual(PIDType.CXD, info.Type, "Unable to match PID to CXD spread!");
                    if (!GetCanOriginateFrom(team, info, false, out var _))
                        continue;
                    match = true;
                    break;
                }
                Assert.IsTrue(match, $"Unable to verify lock conditions for result {i}: " + team[0].Species);
            }

            IEnumerable<uint> getSeeds(uint PID)
            {
                var top = PID >> 16;
                var bot = PID & 0xFFFF;

                var seeds = MethodFinder.GetSeedsFromPIDEuclid(RNG.XDRNG, top, bot);
                foreach (var s in seeds)
                    yield return RNG.XDRNG.Reverse(s, 3);
            }
        }

        /// <summary>
        /// Checks if the PIDIV can originate from
        /// </summary>
        /// <param name="possibleTeams"></param>
        /// <param name="info"></param>
        /// <returns></returns>
        private static bool GetCanOriginateFrom(TeamLock[] possibleTeams, PIDIV info, bool XD, out uint origin)
        {
            foreach (var team in possibleTeams)
            {
                var result = LockFinder.FindLockSeed(info.OriginSeed, team.Locks, XD, out origin);
                if (result)
                    return true;
            }
            origin = 0;
            return false;
        }
    }
}