using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EvernoteSDK;
using EvernoteSDK.Advanced;
using Evernote.EDAM.NoteStore;

namespace EvernoteSdkLab
{
    class Program
    {
        static void Main(string[] args)
        {
            //https://www.evernote.com/api/DeveloperToken.action
            ENSessionAdvanced.SetSharedSessionDeveloperToken("", "https://www.evernote.com/shard/s66/notestore");
            var tags = ENSessionAdvanced.SharedSession.PrimaryNoteStore.ListTags().Where(t => t.Name.StartsWith("*")).ToList();
            foreach (var t in tags)
            {
                if (t.Name != "*2014")
                    continue;
                Console.WriteLine(t.Name);
                var f = new NoteFilter();
                f.TagGuids = new List<string>();
                f.TagGuids.Add(t.Guid);
                var notes = ENSessionAdvanced.SharedSession.PrimaryNoteStore.FindNotes(f, 0, 99).Notes;
                foreach (var note in notes)
                {
                    Console.WriteLine(note.Title);
                    int year = Convert.ToInt32(t.Name.Replace("*", ""));
                    note.Created = ConvertDateTimeToEvernoteDate(new DateTime(year, 1, 1));
                    note.Updated = ConvertDateTimeToEvernoteDate(new DateTime(year, 1, 1));
                    ENSessionAdvanced.SharedSession.PrimaryNoteStore.UpdateNote(note);
                }
            }

            Console.ReadLine();
        }

        //https://discussion.evernote.com/topic/3448-converting-the-long-dates-to-date-times/
        public static long ConvertDateTimeToEvernoteDate(DateTime date)
        {
            // adjust for the current timezone
            date = date.ToUniversalTime();
            // get the ticks as a base for the standard web base date
            long baseOffset = new DateTime(1970, 1, 1).Ticks;
            // get the difference between the base and our date
            long evDate = date.Ticks - baseOffset;
            // convert from ticks to seconds
            evDate = evDate / 10000;

            return evDate;
        }

        public static DateTime ConvertEvernoteDateToDateTime(long evDate)
        {
            // convert from seconds to ticks
            TimeSpan ts = new TimeSpan((evDate * 10000));
            // create a date with the standard web base of 01/01/1970
            // add the timespan difference
            DateTime date = new DateTime(1970, 1, 1).Add(ts);
            // adjust for the current timezone
            ts = TimeZone.CurrentTimeZone.GetUtcOffset(date);
            date = date.Add(ts);

            return date;
        }

    }
}
