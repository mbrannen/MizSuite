using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace MizSuite.ConsoleApp
{
    public class Airframe
    {
        public List<LineClass> Lines { get; set; }
        public string Type { get; set; }
        public int BeginIndex { get; set; }
        public int EndIndex { get; set; }

        public List<Radio> Radios { get; set; }

        public Airframe(LineClass begin, LineClass end, ref List<LineClass> content)
        {
            BeginIndex = begin.Index;
            EndIndex = end.Index;
            Lines = RemoveModulations(content.Where(x => x.Index > BeginIndex && x.Index < EndIndex).ToList());
            Type = GetType(content[begin.Index - 1].Content);
            Radios = new List<Radio>();
            SetRadios(Lines);
        }

        private List<LineClass> RemoveModulations(List<LineClass> lines)
        {
            var modulationsStart = lines.Where(l => l.Content.Contains("[\"modulations\"] =")).ToList();
            var modulationsEnd = lines.Where(l => l.Content.Contains("-- end of [\"modulations\"]")).ToList();

            for(int i = 0; i < modulationsStart.Count(); i++)
            {
                lines = lines.Where(l => l.Index < modulationsStart[i].Index || l.Index > modulationsEnd[i].Index).ToList();
            }
            return lines;
        }

        private string GetType(string type)
        {
            Regex regex = new Regex("\"(.*?)\"");

            var matches = regex.Matches(type);

            return matches[1].Value.Replace("\"","");
        }

        private void SetRadios(List<LineClass> lines)
        {
            Regex radioBeginRegex = new Regex(@"\[[0-9]\] = $");
            Regex radioEndRegex = new Regex(@"-- end of \[[0-9]\]");

            List<LineClass> radioBegin = lines.Where(x => radioBeginRegex.IsMatch(x.Content)).OrderBy(x => x.Index).ToList();
            List<LineClass> radioEnd = lines.Where(x => radioEndRegex.IsMatch(x.Content)).OrderBy(x => x.Index).ToList();

            for(int i = 0; i < radioBegin.Count; i++)
            {
                Regex digit = new Regex(@"\d");
                var match = digit.Matches(radioBegin[i].Content);
                var radioNumber = int.Parse(match[0].Value);
                Radios.Add(new Radio(lines.Where(l => l.Index > radioBegin[i].Index && l.Index < radioEnd[i].Index).OrderBy(l=>l.Index).ToList(), radioNumber));
            }
        }
    }

    public class Channel
    {
        public LineClass Content { get; set; }
        public int Number { get; set; }
        public double Frequency { get; set; }
        public bool HasChanged { get; set; } = false;

    }

    public class Radio
    {
        public List<LineClass> Content { get; set; }
        public List<Channel> Channels { get; set; }
        public int Number { get; set; }

        public Radio(List<LineClass> content, int number )
        {
            Content = content;
            Number = number;
            Channels = new List<Channel>();
            SetChannels(Content);
        }

        private void SetChannels(List<LineClass> lines)
        {
            Regex channelLineRegex = new Regex(@"\[[0-9]{1,2}\] = [0-9.]+");
            List<LineClass> channelLines = lines.Where(l => channelLineRegex.IsMatch(l.Content)).OrderBy(l=>l.Index).ToList();

            foreach(LineClass line in channelLines)
            {
                Regex channelNumber = new Regex(@"(?!\[)[0-9]{1,2}(?=\])");
                Regex frequencyNumber = new Regex(@"([0-9.])+(?=,)");
                var numberTest = channelNumber.Match(line.Content).Value;
                var frequencyTest = frequencyNumber.Match(line.Content).Value;

                Channels.Add(new Channel
                {
                    Content = line,
                    Number = int.Parse(channelNumber.Match(line.Content).Value),
                    Frequency = double.Parse(frequencyNumber.Match(line.Content).Value)
            });
            }
        }


    }
}
