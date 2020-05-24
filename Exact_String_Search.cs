using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace String_Search
{
    class Program
    {
        struct P_Bad_Character_struct
        {
            public char P_Char;
            public int Bad_Character_Shift;

            public P_Bad_Character_struct(char input1, int input2)
            {
                P_Char = input1;
                Bad_Character_Shift = input2;
            }
        }

        static void Main(string[] args)
        {
            //string T = "GTTATAGCTGATCGCGGCGTAGCGGCGATAT";  //original
            //string T = "GTTATAGCTGATCCCGGCGTAGCGGCGATATCTCCCCC";
            //string T = "GTTAGAGCTGATCGCGGCGTAGCGGCGATATCGAGCGGCGCCTCATAGTAGATA";
            string T = "TGCATGTTAGAGTGATGAAGCGATAAAAGGTAGGTAGCGGCGTAGGAAAACCGTGATAGTAGAAAAATATAGATAAGATACGCAATTACA";  //
            //string T = "CGTGCCTACTTACTTACTTACTTACGCGAA";  //
            //string P = "CTTACTTAC";
            //string P = "GTAGCGGCG";  //original
            //string T = "GTTATAGCTGATCGCGGCGTAGCGGCGAA";   //original
            //string P = "GTTA";
            //string P = "CGTAG";
            //string P = "GCGG";
            //string P = "CGTA";
            //string P = "GCGA";
            //string P = "TGATCGC";
            //string P = "GCGATAT";
            //string P = "ATAGTAGATA";
            //string P = "TAGATAAGATA";
            //string P = "AAA";
            //string P = "CGCAATTACA";    //10 characters
            //string P = "CC";
            //string P = "A";
            //string P = "TC";
            //string P = "CCTC";
            //string P = "CTCCCCC";
            //string P = "ATC";
            //string P = "AAAA";
            string P = "AAAAA";

            int P_position_in_T = string_search(P, T);
            if (P_position_in_T == -1)
                Console.WriteLine("P string not exist in string T");
            else  //P_position_in_T >= 0
                Console.WriteLine("String P is found inside string T, between " + P_position_in_T + " to " + (P_position_in_T + P.Length - 1));

            Console.Read();
        }

        private static int string_search(string P, string T)
        {
            string matched_string = "";
            int start_P_in_T = 0;
            int end_P_in_T = P.Length - 1;
            int T_ptr;

            //Check string 'P' & 'T' are not empty and T string is longer than P string, or else definitely will not be any matches and no reason to proceed the checking
            if (!String.IsNullOrEmpty(P) && !String.IsNullOrEmpty(T) && T.Length >= P.Length)
            {
                if (P.Length == 1)	//Special case: if string 'P' only consist of 1 character
                {
                    T_ptr = 0;
                    while (T_ptr < T.Length && P[0] != T[T_ptr])
                        T_ptr++;

                    if (T_ptr != T.Length)
                        return T_ptr;
                    else
                        return -1;
                }

                int P_ptr;  //Pointer showing position of target search string 'P' last character now
                T_ptr = P.Length - 1;  //Pointer showing position of target search string 'P' last character in string 'T' now

                //To optimize quick shifting, contruct string 'P' number of shift array table
                //First case: when only last character of string 'P' matching string 'T', how many characters shift string 'P' should be in string 'T'
                P_ptr = P.Length - 2;
                int only_last_character_of_P_matching_shift;
                while (P_ptr >= 0 && P[P_ptr] != P.Last())
                    P_ptr--;
                only_last_character_of_P_matching_shift = P.Length - 1 - P_ptr;

                //Second case: when string 'P' suffix partially matching string 'T', 2 characters & more. How many characters shift string 'P' should be in string 'T'
                matched_string = P.Substring(P.Length - 2, 2);
                int matched_string_ptr = 1;

                //P_ptr pointer continue from only_last_character_of_P_matching case
                int[] P_Good_Suffix_shift_table = new int[P.Length - 2];

                //P_Good_Suffix_Rule
                finding_good_suffix_shift:
                if (P_ptr >= 0)
                {
                    finding_good_suffix_shift_2:
                    if (matched_string[matched_string_ptr] == P[P_ptr])
                    {
                        if (P_ptr == 0 || matched_string_ptr == 0)
                            goto found_good_suffix_shift;

                        matched_string_ptr--;
                    }
                    else
                    {
                        if (matched_string_ptr != matched_string.Count() - 1)
                        {
                            matched_string_ptr = matched_string.Count() - 1;
                            goto finding_good_suffix_shift_2;
                        }
                    }

                    P_ptr--;

                    goto finding_good_suffix_shift;
                }

                found_good_suffix_shift:
                if (P_ptr > 0)       //Match string are found in the mid of remaining P string
                {
                    P_Good_Suffix_shift_table[matched_string.Length - 2] = P.Length - matched_string.Length - P_ptr;
                    P_ptr--;

                    //matched_string increase length by 1
                    matched_string = P.Substring(P.Length - matched_string.Length - 1, matched_string.Length + 1);  //matched_string increase length by 1
                    matched_string_ptr = 0;  //search suffix from previous shorter suffix position, 1 character to the left
                    goto finding_good_suffix_shift;  //should goto finding_good_suffix_shift_2
                }
                else if (P_ptr == 0) //Match string are found in the start of remaining P string
                {
                    int P_Good_Suffix_shift = P.Length - matched_string.Count() + matched_string_ptr;
                    for (int i = matched_string.Length - 2; i < P.Length - 2; i++)
                        P_Good_Suffix_shift_table[i] = P_Good_Suffix_shift;
                    //goto P_Bad_Character_Rule;
                }
                else  //When P_ptr < 0, no match found in remaining P string
                {
                    for (int i = matched_string.Length - 2; i < P.Length - 2; i++)
                        P_Good_Suffix_shift_table[i] = P.Length;
                    //goto P_Bad_Character_Rule;
                }


                //Third case: when string 'P' last character does not match string 'T'
                //P_Bad_Character_Rule
                P_ptr = P.Length - 2;
                List<P_Bad_Character_struct> P_Bad_Character_shift_table = new List<P_Bad_Character_struct>();
                while (P_ptr >= 0 && P[P_ptr] == P.Last())
                    P_ptr--;
                //P_ptr == -1, special case when string 'P' only consist of only one duplicated character
                if (P_ptr != -1) //string 'P' has character different from P.Last()
                {
                    P_Bad_Character_shift_table.Add(new P_Bad_Character_struct(P[P_ptr], P.Length - 1 - P_ptr)); //Add P second last character into the table
                    P_ptr--;

                    while (P_ptr >= 0)  //when string P.length >= 3
                    {
                        int i = 0;
                        while (i < P_Bad_Character_shift_table.Count() && P_Bad_Character_shift_table[i].P_Char != P[P_ptr])
                            i++;

                        if (i == P_Bad_Character_shift_table.Count() && P[P_ptr] != P.Last()) //P current character not occur inside P_Bad_Character_shift_table, add this new character & it position
                            P_Bad_Character_shift_table.Add(new P_Bad_Character_struct(P[P_ptr], P.Length - 1 - P_ptr));

                        P_ptr--;
                    }

                    for (int i = 0; i < P_Bad_Character_shift_table.Count(); i++)
                        Console.WriteLine("P_Bad_Character_shift_table.P_Char[" + i + "] = " + P_Bad_Character_shift_table[i].P_Char
                            + ", Bad_Character_Shift = " + P_Bad_Character_shift_table[i].Bad_Character_Shift);
                    for (int i = 0; i < P_Good_Suffix_shift_table.Count(); i++)
                        Console.WriteLine("P_Good_Suffix_shift_table[" + i + "] = " + P_Good_Suffix_shift_table[i]);
                }

                //Start string 'P' against string 'T' comparison
                int matched_string_length = 0;
                P_ptr = P.Length - 1;

                string_compare_P_against_T:
                if (T_ptr < T.Length)
                {
                    //String compare from string 'P' last character toward first character against string 'T'. Stop whenever there is a mismatch.
                    while (P_ptr >= 0 && P[P_ptr] == T[T_ptr])
                    {
                        matched_string_length++;
                        T_ptr--;
                        P_ptr--;
                    }

                    if (matched_string_length == 0)  //Bad Character Rule
                    {
                        if (P_Bad_Character_shift_table.Count() == 0)
                        {
                            //special case when string 'P' only consist of only one duplicated character
                            start_P_in_T += P.Length;
                            end_P_in_T += P.Length;
                        }
                        else
                        {
                            int i = 0;
                            while (i < P_Bad_Character_shift_table.Count() && P_Bad_Character_shift_table[i].P_Char != T[end_P_in_T])
                                i++;

                            if (i == P_Bad_Character_shift_table.Count())
                            {
                                //string 'T' character correspond to string 'P' last character position cannot be found in string 'P'
                                start_P_in_T += P.Length;
                                end_P_in_T += P.Length;
                            }
                            else
                            {
                                start_P_in_T += P_Bad_Character_shift_table[i].Bad_Character_Shift;
                                end_P_in_T += P_Bad_Character_shift_table[i].Bad_Character_Shift;
                            }
                        }
                        Console.Write("Bad Character Rule, ");
                    }
                    else if (matched_string_length == 1)
                    {
                        //Only P last character match
                        start_P_in_T += only_last_character_of_P_matching_shift;
                        end_P_in_T += only_last_character_of_P_matching_shift;
                        Console.Write("Only P last character match, ");
                    }
                    else if (matched_string_length == P.Length) //found string 'P' matching string 'T' in this position
                        return start_P_in_T;    //return the position of string 'P' in string 'T'
                    else  //P against T matched string is 2 characters or more but not exactly matched, only partially match
                    {
                        //Good Suffix Rule
                        start_P_in_T += P_Good_Suffix_shift_table[matched_string_length - 2];
                        end_P_in_T += P_Good_Suffix_shift_table[matched_string_length - 2];
                        Console.Write("Good Suffix Rule, ");
                    }

                    T_ptr = end_P_in_T;
                    P_ptr = P.Length - 1;
                    matched_string_length = 0;
                    Console.WriteLine("start_P_in_T = " + start_P_in_T);

                    goto string_compare_P_against_T;
                }
            }

            return -1;  //found string 'P' not matching string 'T'
        }
    }
}
