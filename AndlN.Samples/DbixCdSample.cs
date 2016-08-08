// translation of sample application from http://search.cpan.org/dist/DBIx-Class/lib/DBIx/Class/Manual/Example.pod
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AndlN;

namespace AndlN.Samples {
  public class DbixCdSample {
    public static void Run() {
      var artist = Relatable.FromSource(Artist.Data());
      var cd = Relatable.FromSource(Cd.Data());
      var track = Relatable.FromSource(Track.Data());

      Show("artist", artist);
      Show("cd", cd);
      Show("track", track);

      Show("");
      Show("get_tracks_by_cd(Bad):",
        cd.Where(t => t.cdtitle == "Bad")
        .Join(track, t => new { t.title })
        .AsEnumerable()
        .Select(t => $"{t.title}"));
      Show("");
      Show("get_tracks_by_artist(Michael Jackson):",
        artist.Where(t => t.name == "Michael Jackson")
        .Join(cd, o => new { o.cdid, o.cdtitle })
        .Join(track, (t,o)=> new { t.cdtitle, o.title })
        .AsEnumerable()
        .Select(t => $"{t.title} (from the CD '{t.cdtitle}')"));
      Show("");
      Show("get_cd_by_track(Stan):",
        track.Where(t => t.title=="Stan")
        .Join(cd, (t,o)=> new { t.title, o.cdtitle })
        .AsEnumerable()
        .Select(t => $"{t.cdtitle} has the track '{t.title}'"));
      Show("");
      Show("get_cds_by_artist(Michael Jackson):",
        artist.Where(t => t.name == "Michael Jackson")
        .Join(cd, o => new { o.cdtitle })
        .AsEnumerable()
        .Select(t => $"{t.cdtitle}"));
      Show("");
      Show("get_artist_by_track(Dirty Diana):",
        track.Where(t => t.title== "Dirty Diana")
        .Join(cd, (t,o) => new {t.title, o.artistid})
        .Join(artist, (t, o) => new { t.title, o.name})
        .AsEnumerable()
        .Select(t => $"{t.name} recorded the track '{t.title}'"));
      Show("");
      Show("get_artist_by_cd(The Marshall Mathers LP):",
      cd.Where(t => t.cdtitle == "The Marshall Mathers LP")
        .Join(cd, (t, o) => new { t.cdtitle, o.artistid })
        .Join(artist, (t, o) => new { t.cdtitle, o.name })
        .AsEnumerable()
        .Select(t => $"{t.name} recorded the CD '{t.cdtitle}'"));
      Show("");
    }

    public static void Show<T>(string msg, IRelatable<T> srel) where T : class {
      Console.WriteLine(msg);
      foreach (var s in srel)
        Console.WriteLine($"  {TupleType.Format(s)}");
    }
    public static void Show<T>(string msg, IEnumerable<T> srel)  {
      Console.WriteLine(msg);
      foreach (var s in srel)
        Console.WriteLine(s);
    }
    public static void Show(string msg) { 
      Console.WriteLine(msg);
    }
  }

  public class Artist {
    public int artistid;
    public string name;
    public static Artist[] Data() {
      return new Artist[] {
          new Artist { artistid = 1, name= "Michael Jackson" },
          new Artist { artistid = 2, name= "Eminem" },
        };
    }
  }

  public class Cd {
    public int cdid;
    public int artistid;
    public string cdtitle;
    public int year;
    public static Cd[] Data() {
      return new Cd[] {
        new Cd { cdid = 0, artistid = 1, cdtitle = "Thriller" },
        new Cd { cdid = 1, artistid = 1, cdtitle = "Bad" },
        new Cd { cdid = 2, artistid = 2, cdtitle = "The Marshall Mathers LP" },
      };
    }
  }

  public class Track {
    public int trackid;
    public int cdid;
    public string title;
    public static Track[] Data() {
      return new Track[] {
      new Track { trackid = 0, title= "Beat It"         , cdid = 0 },
      new Track { trackid = 1, title= "Billie Jean"     , cdid = 0 },
      new Track { trackid = 2, title= "Dirty Diana"     , cdid = 1 },
      new Track { trackid = 3, title= "Smooth Criminal" , cdid = 1 },
      new Track { trackid = 4, title= "Leave Me Alone"  , cdid = 1 },
      new Track { trackid = 5, title= "Stan"            , cdid = 2 },
      new Track { trackid = 6, title= "The Way I Am"    , cdid = 2 },
    };
    }
  }
}

