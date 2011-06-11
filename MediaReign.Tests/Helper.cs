using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MediaReign.Tests {
	public class Helper {

		public class Show {
			public string File { get; set; }
			public string Name { get; set; }
			public int? Season { get; set; }
			public int Episode { get; set; }
			public bool IsAbsolute { get; set; }
			public bool IsDummy { get; set; }
		}

		public static Show[] Shows = new[] {
			new Show {
				File = @"Come.Fly.With.Me.2010.S01E01.HDTV.XviD-ANGELiC.avi",
				Name = "Come Fly With Me 2010",
				Season = 1,
				Episode = 1
			},
			new Show {
				File = @"doctor_who_2005.6x06.the_almost_people.hdtv_xvid-fov.avi",
				Name = "Doctor Who 2005",
				Season = 6,
				Episode = 6
			},
			new Show { 
				File = @"Game.of.Thrones.S01E07.720p.HDTV.x264-IMMERSE.mkv",
				Name = "Game of Thrones",
				Season = 1,
				Episode = 7
			},
			new Show { 
				File = @"Priest.2011.R5.LiNE.AC3.XViD-EP1C.avi",
				IsDummy = true
			},
			new Show { 
				File = @"South.Park.S15E06.City.Sushi.HDTV.XviD-FQM.avi",
				Name = "South Park",
				Season = 15,
				Episode = 6
			},
			new Show { 
				File = @"Fallout 3.iso",
				IsDummy = true
			},
			new Show { 
				File = @"The.Biggest.Loser.Australia.Families.s06e53.Finale.PDTV.Xvid.avi",
				Name = "The Biggest Loser Australia Families",
				Season = 6,
				Episode = 53
			},
			new Show { 
				File = @"[SFW-Chihiro]_Dance_in_the_Vampire_Bund_-_01_[1280x720_Blu-ray_FLAC][48EAB09D].mkv",
				Name = "Dance in the Vampire Bund",
				Episode = 1,
				IsAbsolute = true
			},
			new Show { 
				File = @"The Private Life of Plants - 01 - Travelling.avi",
				Name = "The Private Life of Plants",
				Episode = 1,
				IsAbsolute = true
			},
			new Show { 
				File = @"(HBO) The Private Life of Plants - 212 - Travelling.avi",
				Name = "The Private Life of Plants",
				Season = 2,
				Episode = 12
			},
			new Show {
				File = @"Spriggan.1998.x264.BDRip(720)_HDClub.mkv",
				IsDummy = true
			}
		};
	}
}
