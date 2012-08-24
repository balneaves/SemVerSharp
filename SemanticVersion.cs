using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Text.RegularExpressions;

namespace SemVerSharp
{
    public class SemanticVersion
    {
        private static readonly Regex VersionStringFormat = new Regex(@"^([0-9]+)\.([0-9]+)\.([0-9]+)([-+]{1}[a-zA-Z0-9-\.:]*)?$", RegexOptions.Compiled);

        public int Major { get; private set; }
        public int Minor { get; private set; }
        public int Patch { get; private set; }

        private readonly List<string> preReleaseParts;
        private ReadOnlyCollection<string> PreRelease
        {
            get { return preReleaseParts.AsReadOnly(); }
        }

        private readonly List<string> buildParts;

        public ReadOnlyCollection<string> Build
        {
            get { return buildParts.AsReadOnly(); }
        }

        public bool IsPreRelease
        {
            get { return ((preReleaseParts != null) && (preReleaseParts.Count > 0)); }
        }

        public bool IsBuild
        {
            get { return ((buildParts != null) && (buildParts.Count > 0)); }
        }

        public SemanticVersion(int major, int minor, int patch)
        {
            Major = major;
            Minor = minor;
            Patch = patch;
        }

        public SemanticVersion(int major, int minor, int patch, int build)
            : this(major, minor, patch)
        {
            buildParts = new List<string> { "build", build.ToString(CultureInfo.InvariantCulture) };
        }

        public static SemanticVersion Parse(string version)
        {
            var match = VersionStringFormat.Match(version);
            if (match.Success)
            {
                var semanticVersion = new SemanticVersion
                    (
                         Int32.Parse(match.Groups[0].Value),
                         Int32.Parse(match.Groups[1].Value),
                         Int32.Parse(match.Groups[2].Value)
                    );

                if (match.Groups[3].Success)
                {
                    if (match.Groups[3].Value.StartsWith("-"))
                    {

                    }
                    else
                    {

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

        public override string ToString()
        {
            var primary = String.Format("{0}.{1}.{2}", Major, Minor, Patch);

            if (IsPreRelease)
            {
                primary += String.Format("-{0}", String.Join(".", preReleaseParts));
            }
            else if (IsBuild)
            {
                primary += String.Format("+{0}", String.Join(".", buildParts));
            }

            return primary;
        }
    }
}
