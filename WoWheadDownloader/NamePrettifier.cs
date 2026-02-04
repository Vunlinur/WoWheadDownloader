using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace WoWheadDownloader {
	internal class NamePrettifier {
		public static string Prettify(string name) {
			// e.g. SPELL_8.0_Warfronts_Arathi_H_Eitrigg_Ravager_Aura_Loop
			// AMB_80_Warfronts_Arathi_LumberYard_SawLoop (Shredder)
			// MUS_SouthBarrens_GN

			// drop the brackets suffix - some sounds have more spaces in the name, but it's an edge case
			IEnumerable<string> fileNameParts = name.Split();
			if (fileNameParts.Count() > 1)
				fileNameParts = fileNameParts.SkipLast(1);

			// split into words by '_'
			fileNameParts = fileNameParts.SelectMany(p => p.Split('_'));

			// drop version and other numbers-only sections
			if (fileNameParts.Count() > 2) {  // safety check e.g. for FILEDATA_6183787
				fileNameParts = fileNameParts.Where(p => !p.Replace(".", "").All(char.IsDigit));

				// drop the type prefix
				fileNameParts = fileNameParts.Skip(1);
			}

			// split into words by camel case
			fileNameParts = fileNameParts.SelectMany(p => Regex.Replace(p, "(?<=[a-z])([A-Z])", " $0", RegexOptions.Compiled).Trim().Split());

			// drop parts that are too short to be meaningful
			fileNameParts = fileNameParts.Where(p => p.Length >= 2);

			// drop parts that are short but can be meaningful, but are all in CAPS
			fileNameParts = fileNameParts.Where(p => !(p.Length <= 3 && p.All(char.IsUpper)));

			// capitalize the first letter of each word
			string[] exceptions = ["of", "the", "and", "in", "on", "at", "to", "for", "with", "from", "by", "as", "or", "but", "nor"];
			fileNameParts = fileNameParts.Select(p => exceptions.Contains(p) ? p : p[..1].ToUpper() + p[1..].ToLower());

			// concat the parts
			string fileName = string.Join(" ", fileNameParts);
			return fileName;
		}


		private static readonly IEnumerable<string> testData = @"FILEDATA_6183787
LEVELUP
TrollMaleMainDeath
igPlayerInvite
iSelectTarget
Tailoring
Fishing Hooked
classic_Mace2H_ArmorFlesh
FrostWardTarget (Hodir)
NightElf Female Vocal 16 (Than
SpiritWolf (DONOTRENAME)
Zone-Stormwind
ClientScene_71_Karazhan_Vision3_ArcaneGolem_Impact
A_COIL_Thesp_Aggro02
MON_MechaDevilsaur_Fidget0_01_277844
VO_91_Arthas_10_M
VO_110_Xalatath_Blade_of_the_Black_Empire_35_F
VO_83_PC_Vulpera_Female_54_F
VO_801_KUL_TIRAN_COMMONER_M_ATTACKCRIT
SPELL_83_Vexiona_TwilightDecimator_VFXAura_Loop_Fire_Base
SPELL_8.2_Azerite_Essence_The_Ever-Rising_Tide_Cast_Rank4_part2
11.0_Hallowfall_Crystal_Shift_Light_Edit_04
11.1 Delves - Old God - Xel'anegh the Many - Void Tentacle - State Persistant".Split("\r\n").Where(s => !string.IsNullOrEmpty(s));
	}
}
