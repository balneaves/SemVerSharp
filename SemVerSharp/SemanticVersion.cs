using System;
using System.Linq;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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
                return parts.Count > 4 ? parts.GetRange(4, parts.Count - 4).Select(part => part.ToString()).ToList().AsReadOnly() : new List<string>().AsReadOnly();
            }
        }

        public bool IsStable { get { return ((int)parts[3]) == 1; } }
        public bool IsPreRelease { get { return ((int)parts[3]) == 0; } }
        public bool IsBuild { get { return ((int)parts[3]) == 2; } }

        public SemanticVersion(int major, int minor, int patch)
        {
            parts.Add(major);
            parts.Add(minor);
            parts.Add(patch);

            //Add the stable flag to allow prerelease/build/stable comparison
            parts.Add(1);
        }


        /// <summary>
        /// Creates a version from a standard windows/.net version number
        /// </summary>
        public SemanticVersion(int major, int minor, int patch, int build)
        {
            parts.Add(major);
            parts.Add(minor);
            parts.Add(patch);

            parts.Add(2);

            parts.Add("build");
            parts.Add(build);
        }

        public SemanticVersion(string version)
        {
            var match = VersionStringFormat.Match(version);
            if (match.Success)
            {
                parts.Add(Int32.Parse(match.Groups[1].Value));
                parts.Add(Int32.Parse(match.Groups[2].Value));
                parts.Add(Int32.Parse(match.Groups[3].Value));

                if (match.Groups[4].Success)
                {
                    string extraVersion = match.Groups[4].Value;
                    if (extraVersion.StartsWith("-"))
                    {
                        parts.Add(0);
                        extraVersion = extraVersion.TrimStart('-');
                    }
                    else
                    {
                        parts.Add(2);
                        extraVersion = extraVersion.Trim('+');
                    }

                    foreach (var part in extraVersion.Split(new[] { '.' }, StringSplitOptions.RemoveEmptyEntries))
                    {
                        int value;
                        if (Int32.TryParse(part, out value))
                        {
                            parts.Add(value);
                        }
                        else
                        {
                            parts.Add(part);
                        }
                    }
                }
                else
                {
                    parts.Add(1);
                }
            }
            else
            {
                //TODO : better, more discriptive error message with link to semver.org
                throw new FormatException("Version is not in the correct format");
            }

        }
        public static SemanticVersion Parse(string version)
        {
            return new SemanticVersion(version);
        }

        public static bool TryParse(string version, out SemanticVersion semanticVersion)
        {
            try
            {
                if (IsValid(version))
                {
                    semanticVersion = new SemanticVersion(version);
                    return true;
                }
                semanticVersion = null;
                return false;
            }
            catch
            {
                semanticVersion = null;
                return false;
            }
        }

        public static bool IsValid(string version)
        {
            return VersionStringFormat.IsMatch(version);
        }

        public int CompareTo(SemanticVersion other)
        {
            for (int i = 0; i < (other.parts.Count > parts.Count ? other.parts.Count : parts.Count); i++)
            {
                //If this has more parts to other and was equal up to now, then this is later version
                if (i >= other.parts.Count)
                {
                    return 1;
                }
                //If other has more parts than this and was equal up to now, then other is a later version
                if (i >= parts.Count)
                {
                    return -1;
                }

                //If both parts are Int32 compare as numeric
                if ((parts[i] is Int32) && (other.parts[i] is Int32))
                {
                    int result = ((int)parts[i]).CompareTo((int)other.parts[i]);
                    if (result != 0)
                    {
                        return result;
                    }
                }
                else
                {
                    int result = String.Compare(parts[i].ToString(), other.parts[i].ToString(), StringComparison.Ordinal);
                    if (result != 0)
                    {
                        return result;
                    }
                }
            }

            //If we get this far then everything was identical
            return 0;
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

        public override bool Equals(object obj)
        {
            var rhs = obj as SemanticVersion;
            if (rhs != null)
            {
                return CompareTo(rhs) == 0;
            }

            return false;
        }

        public override int GetHashCode()
        {
            return ToString().GetHashCode();
        }
    }
}
