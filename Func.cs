using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace OpenSMO {
    public static class Func {
        public static string RepeatChar(char chr, int count) {
            string ret = "";
            for (int i = 0; i < count; i++)
                ret += chr;
            return ret;
        }

        public static string ChatColor(string colorCode) {
            if (colorCode.Length == 3) colorCode = RepeatChar(colorCode[0], 2) + RepeatChar(colorCode[1], 2) + RepeatChar(colorCode[2], 2);
            return "|c0" + colorCode;
        }

        public static int GetStepWeight(NSNotes note) {
            switch (note) {
                case NSNotes.Mine: return -8;
                case NSNotes.Miss: return -8;
                case NSNotes.Barely: return -4;
                case NSNotes.Great: return 1;
                case NSNotes.Perfect:
                case NSNotes.Flawless: return 2;
                case NSNotes.Held: return 6;
                default: return 0;
            }
        }

        public static float GetPercTier(int i) {
            switch (i) {
                case 1: return 1.00f;
                case 2: return 0.99f;
                case 3: return 0.97f;
                case 4: return 0.93f;
                case 5: return 0.80f;
                case 6: return 0.65f;
                case 7: return 0.45f;
                case 8: return -99999.00f;
                default: return 0f;
            }
        }

        public static NSGrades GetGrade(int[] notes) {
            int gradePoints = GetGradePoints(notes);
            int gradePointsMax = GetMaxGradePoints(notes);

            float perc = gradePoints / (float)gradePointsMax;
            return GetGrade(gradePoints / (float)gradePointsMax);
        }

        private static NSGrades GetGrade(float perc) {
            for (int i = 1; i < (int)NSGrades.NUM_NS_GRADES; i++) {
                if (perc >= GetPercTier(i))
                    return (NSGrades)(i - 1);
            }
            return NSGrades.F;
        }

        public static int GetGradePoints(int[] notes) {
            int ret = 0;
            for (int i = 0; i < (int)NSNotes.NUM_NS_NOTES; i++)
                ret += notes[i] * GetStepWeight((NSNotes)i);
            return ret;
        }

        public static int GetMaxGradePoints(int[] notes) {
            int ret = 0;
            int flawlessWeight = GetStepWeight(NSNotes.Flawless);
            for (int i = 0; i < (int)NSNotes.NUM_NS_NOTES; i++)
                ret += notes[i] * flawlessWeight;
            return ret + (notes[(int)NSNotes.Held] + notes[(int)NSNotes.NG]) * GetStepWeight(NSNotes.Held);
        }
    }
}
