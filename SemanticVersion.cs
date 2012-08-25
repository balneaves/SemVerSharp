using System;
using System.Linq;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Text.RegularExpressions;

namespace SemVerSharp
{
    public class SemanticVersion : IComparable<SemanticVersion>
    {
        private static readonly Regex VersionStringFormat = new Regex(@"^([0-9]+)\.([0-9]+)\.([0-9]+)([-+]{1}[a-zA-Z0-9-\.:]*)?$", RegexOptions.Compiled);

        public int Major { get { return (int)parts[0]; } }
        public int Minor { get { return (int)parts[1]; } }
        public int Patch { get { return (int)parts[2]; } }

        private readonly List<object> parts = new List<object>();

        public ReadOnlyCollection<string> VersionInformation
        {
            get
            {
                return parts.Count > 3 ? parts.GetRange(3, parts.Count - 3).Select(part => part.ToString()).ToList().AsReadOnly() : new List<string>().AsReadOnly();
            }
        }

        public bool IsPreRelease { get; private set; }
        public bool IsBuild { get; private set; }

        public SemanticVersion(int major, int minor, int patch)
        {
            parts.Add(major);
            parts.Add(minor);
            parts.Add(patch);

            IsBuild = false;
            IsPreRelease = false;
        }


        /// <summary>
        /// Creates a version from a standard windows/.net version number
        /// </summary>
        public SemanticVersion(int major, int minor, int patch, int build)
            : this(major, minor, patch)
        {
            IsBuild = true;

            parts.Add("build");
            parts.Add(build);
        }

        public static SemanticVersion Parse(string version)
        {
            var match = VersionStringFormat.Match(version);
            if (match.Success)
            {
                var semanticVersion = new SemanticVersion
                    (
                         Int32.Parse(match.Groups[1].Value),
                         Int32.Parse(match.Groups[2].Value),
                         Int32.Parse(match.Groups[3].Value)
                    );

                if (match.Groups[3].Success)
                {
                    string extraVersion = match.Groups[4].Value;
                    if (extraVersion.StartsWith("-"))
                    {
                        semanticVersion.IsPreRelease = true;
                        extraVersion = extraVersion.TrimStart('-');
                    }
                    else
                    {
                        semanticVersion.IsBuild = true;
                        extraVersion = extraVersion.Trim('+');
                    }

                    foreach (var part in extraVersion.Split(new[]{'.'}, StringSplitOptions.RemoveEmptyEntries))
                    {
                        int value;
                        if (Int32.TryParse(part, out value))
                        {
                            semanticVersion.parts.Add(value);
                        }
                        else
                        {
                            semanticVersion.parts.Add(part);
                        }
                    }
                }

                return semanticVersion;
            }

            //TODO : better, more discriptive error message with link to semver.org
            throw new FormatException("Version is not in the correct format");
        }

        public bool IsValid(string version)
        {
            return VersionStringFormat.IsMatch(version);
        }

        public int CompareTo(SemanticVersion other)
        {
            throw new NotImplementedException();
        }

        public override string ToString()
        {
            var primary = String.Format("{0}.{1}.{2}", Major, Minor, Patch);

            if (IsPreRelease)
            {
                primary += String.Format("-{0}", String.Join(".", VersionInformation));
            }
            else if (IsBuild)
            {
                primary += String.Format("+{0}", String.Join(".", VersionInformation));
            }

            return primary;
        }
    }
}
