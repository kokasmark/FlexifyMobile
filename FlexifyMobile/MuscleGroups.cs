using System.Collections.Generic;

namespace FlexifyMobile
{
    public class MuscleGroups
    {
        public Dictionary<string, List<int>> Men { get; set; }
        public Dictionary<string, List<int>> Women { get; set; }

        public string[] Front = new string[] {};
        public string[] Back = new string[] {};

        public MuscleGroups()
        {
            Men = new Dictionary<string, List<int>>
            {
                { "traps", new List<int> { 146, 147, 148, 149, 21, 22, 23, 24 } },
                { "shoulder", new List<int> { 26, 27, 29, 30, 150, 151 } },
                { "chest", new List<int> { 25, 28, 31, 32 } },
                { "biceps", new List<int> { 33, 34 } },
                { "triceps", new List<int> { 154, 155, 159, 160, 155, 158, 161 } },
                { "lats", new List<int> { 156, 157 } },
                { "abs", new List<int> { 45, 46, 51, 52, 57, 58, 66, 68 } },
                { "obliques", new List<int> { 37, 38, 39, 40, 41, 42, 44, 43, 44, 47, 49, 48, 50, 61, 62, 170, 171 } },
                { "quadriceps", new List<int> { 71, 73, 91, 104, 72, 74, 92, 103, 75, 77 } },
                { "calves", new List<int> { 206, 207, 208, 209 } },
                { "leg", new List<int> { 105, 113, 117, 107, 115, 119, 106, 114, 118, 108, 116, 120, 212, 210, 211, 213 } },
                { "forearm", new List<int> { 55, 59, 65, 69, 63, 56, 60, 67, 64, 70, 164, 162, 167, 168, 163, 169, 166, 165 } },
                { "glutes", new List<int> { 174, 175, 172, 173, 176, 177 } },
                { "hamstrings", new List<int> { 198, 202, 200, 204, 192, 193, 201, 205, 203, 199 } },
                { "adductors", new List<int> { 76, 79, 87, 93, 94, 88, 82, 78 } },
                { "midBack", new List<int> { 152, 153 } },
                { "neck", new List<int> { 18, 19, 20 } }
            };

            Women = new Dictionary<string, List<int>>
            {
                { "traps", new List<int> { 88, 89 } },
                { "shoulder", new List<int> { 7, 8, 9, 12, 90, 91 } },
                { "chest", new List<int> { 13, 14 } },
                // Add other muscle groups for women
            };

            Front = new string[]
            {
                "shoulder",
                "chest",
                "biceps",
                "lats",
                "abs",
                "obliques",
                "quadriceps",
                "leg",
                "forearm",
                "adductors",
                "neck"

            };

            Back = new string[]
            {
                "traps",
                "shoulder",
                "triceps",
                "calves",
                "forearm",
                "glutes",
                "hamstrings",
                "midBack",
                "neck"

            };
        }
    }
}
