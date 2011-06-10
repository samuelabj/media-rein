using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MediaReign.Tests {
	public class Helper {
		public static string DownloadedPath = @"\\candy-mountain\H\Downloads\Downloaded";
		public static string ApiKey = "A1DA4CF74415C72E";

		public class FileInfo {
			public string File { get; set; }
			public string Show { get; set; }
			public int? Season { get; set; }
			public int Episode { get; set; }
			public bool IsAbsolute { get; set; }
			public bool IsDummy { get; set; }
		}

		public static FileInfo[] Files = new[] {
			new FileInfo {
				File = @"Come.Fly.With.Me.2010.S01E01.HDTV.XviD-ANGELiC.avi",
				Show = "Come Fly With Me 2010",
				Season = 1,
				Episode = 1
			},
			new FileInfo {
				File = @"doctor_who_2005.6x06.the_almost_people.hdtv_xvid-fov.avi",
				Show = "Doctor Who 2005",
				Season = 6,
				Episode = 6
			},
			new FileInfo { 
				File = @"Game.of.Thrones.S01E07.720p.HDTV.x264-IMMERSE.mkv",
				Show = "Game of Thrones",
				Season = 1,
				Episode = 7
			},
			new FileInfo { 
				File = @"Priest.2011.R5.LiNE.AC3.XViD-EP1C.avi",
				IsDummy = true
			},
			new FileInfo { 
				File = @"South.Park.S15E06.City.Sushi.HDTV.XviD-FQM.avi",
				Show = "South Park",
				Season = 15,
				Episode = 6
			},
			new FileInfo { 
				File = @"Fallout 3.iso",
				IsDummy = true
			},
			new FileInfo { 
				File = @"The.Biggest.Loser.Australia.Families.s06e53.Finale.PDTV.Xvid.avi",
				Show = "The Biggest Loser Australia Families",
				Season = 6,
				Episode = 53
			},
			new FileInfo { 
				File = @"[SFW-Chihiro]_Dance_in_the_Vampire_Bund_-_01_[1280x720_Blu-ray_FLAC][48EAB09D].mkv",
				Show = "Dance in the Vampire Bund",
				Episode = 1,
				IsAbsolute = true
			},
			new FileInfo { 
				File = @"The Private Life of Plants - 01 - Travelling.avi",
				Show = "The Private Life of Plants",
				Episode = 1,
				IsAbsolute = true
			},
			new FileInfo { 
				File = @"(HBO) The Private Life of Plants - 212 - Travelling.avi",
				Show = "The Private Life of Plants",
				Season = 2,
				Episode = 12
			}
		};
	}
}
