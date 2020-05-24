using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Runtime.InteropServices;

namespace String_Search
{
    class Program
    {
        [DllImport("kernel32.dll")]
        static extern int GetCurrentProcessorNumber();

        static void Main(string[] args)
        {
            //string T = "GTTATAGCTGATCGCGGCGTAGCGGCGATAT";  //original
            //string T = "GTTATAGCTGATCCCGGCGTAGCGGCGATATCTCCCCC";
            //string T = "GTTAGAGCTGATCGCGGCGTAGCGGCGATATCGAGCGGCGCCTCATAGTAGATA";
            string T = "TGTGCGATAGTAGAAAAATATAGATAAGATACGCAATTACATAGATAAGATACATGTTAGAGTGATGAAGCGATAAAAGGTAGGTAGCGGCGTAGGAAAACCGTGATAGTAGAAAAATATAGATAAGATACGCAATTACA";  //
            //string T = "TGCGATAGTAGAAAAATATAGATAAGATACGCAATTACATAGATAAGATA";  //
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
            string P = "TAGATAAGATA";
            //string P = "AAA";
            //string P = "CGCAATTACA";    //10 characters
            //string P = "CC";
            //string P = "A";
            //string P = "TC";
            //string P = "CCTC";
            //string P = "CTCCCCC";
            //string P = "ATC";
            //string P = "AAAA";
            //string P = "AAAAA";

            Console.WriteLine("The number of processors on this computer is {0}.", Environment.ProcessorCount);
            int number_of_program_threads = Environment.ProcessorCount - 1;   //One thread is reserved for system & other miscellaneous processes

            List<int>[] P_in_T_results = new List<int>[number_of_program_threads];
            for (int i = 0; i < number_of_program_threads; i++)
                P_in_T_results[i] = new List<Int32>();
            List<int> P_in_T_results_total = new List<int>();

            int[] start_index = new int[number_of_program_threads];
            int[] end_index = new int[number_of_program_threads];

            if (P.Length != 1)
            {
                string_search_class my_string_search_class = new string_search_class();

                my_string_search_class.preprocessing_string_P(P);

                start_index[0] = 0;
                for (int i = 0; i < number_of_program_threads; i++)
                {
                    if (i > 0)
                        start_index[i] = end_index[i - 1] - P.Length + 2;
                    end_index[i] = (int)(T.Length / number_of_program_threads * (i + 1) + P.Length - 1);
                }
                end_index[number_of_program_threads - 1] = T.Length - 1;

                Thread[] threadsArray = new Thread[number_of_program_threads];

                for (int i = 0; i < number_of_program_threads; i++)
                {
                    int x = i;
                    threadsArray[i] = new Thread(() => P_in_T_results[x] = my_string_search_class.string_search(P, T.Substring(start_index[x], end_index[x] - start_index[x] + 1)));
                }

                try
                {
                    foreach (Thread t in threadsArray)
                        t.Start();

                    foreach (Thread t in threadsArray)
                        t.Join();
                    // Join both threads with no timeout
                    // Run both until done.
                    // threads have finished at this point.
                }
                catch (ThreadStateException e)
                {
                    Console.WriteLine(e);  // Display text of exception
                }
                catch (ThreadInterruptedException e)
                {
                    Console.WriteLine(e);  // This exception means that the thread
                                           // was interrupted during a Wait
                }
            }
            else //P.Length == 1
            {
                int T_ptr = 0;
                while (T_ptr < T.Length)
                {
                    if (P[0] == T[T_ptr])
                        P_in_T_results_total.Add(T_ptr);

                    T_ptr++;
                }
            }

            for (int x = 0; x < number_of_program_threads; x++)
                for (int y = 0; y < P_in_T_results[x].Count(); y++)
                {
                    P_in_T_results[x][y] += start_index[x];
                }

            for (int i = 0; i < number_of_program_threads; i++)
                P_in_T_results_total.AddRange(P_in_T_results[i]);

            if (P_in_T_results_total.Any())
            {
                Console.Write("String P is found inside string T, between: ");
                foreach (int i in P_in_T_results_total)
                    Console.Write(i + "~" + (i + P.Length - 1) + ", ");
                Console.WriteLine("");
            }
            else
                Console.WriteLine("P string not exist in string T");

            Console.Read();
        }

        public class string_search_class
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

            int only_last_character_of_P_matching_shift;
            int[] P_Good_Suffix_shift_table;
            List<P_Bad_Character_struct> P_Bad_Character_shift_table = new List<P_Bad_Character_struct>();
            int P_Length;

            public void preprocessing_string_P(string local_P)
            {
                string matched_string = "";
                int P_ptr;  //Pointer showing position of target search string 'P' last character now
                P_Length = local_P.Length;

                //To optimize quick shifting, contruct string 'P' number of shift array table
                //First case: when only last character of string 'P' matching string 'T', how many characters shift string 'P' should be in string 'T'
                P_ptr = P_Length - 2;
                while (P_ptr >= 0 && local_P[P_ptr] != local_P.Last())
                    P_ptr--;
                only_last_character_of_P_matching_shift = P_Length - 1 - P_ptr;

                //Second case: when string 'P' suffix partially matching string 'T', 2 characters & more. How many characters shift string 'P' should be in string 'T'
                matched_string = local_P.Substring(P_Length - 2, 2);
                int matched_string_ptr = 1;

                //P_ptr pointer continue from only_last_character_of_P_matching case
                P_Good_Suffix_shift_table = new int[P_Length - 2];

            //P_Good_Suffix_Rule
            finding_good_suffix_shift:
                if (P_ptr >= 0)
                {
                finding_good_suffix_shift_2:
                    if (matched_string[matched_string_ptr] == local_P[P_ptr])
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
                    P_Good_Suffix_shift_table[matched_string.Length - 2] = P_Length - matched_string.Length - P_ptr;
                    P_ptr--;

                    //matched_string increase length by 1
                    matched_string = local_P.Substring(P_Length - matched_string.Length - 1, matched_string.Length + 1);  //matched_string increase length by 1
                    matched_string_ptr = 0;  //search suffix from previous shorter suffix position, 1 character to the left
                    goto finding_good_suffix_shift; //should goto finding_good_suffix_shift_2
                }
                else if (P_ptr == 0) //Match string are found in the start of remaining P string
                {
                    int P_Good_Suffix_shift = P_Length - matched_string.Count() + matched_string_ptr;
                    for (int i = matched_string.Length - 2; i < P_Length - 2; i++)
                        P_Good_Suffix_shift_table[i] = P_Good_Suffix_shift;
                    //goto P_Bad_Character_Rule;
                }
                else  //When P_ptr < 0, no match found in remaining P string
                {
                    for (int i = matched_string.Length - 2; i < P_Length - 2; i++)
                        P_Good_Suffix_shift_table[i] = P_Length;
                    //goto P_Bad_Character_Rule;
                }

                //Third case: when string 'P' last character does not match string 'T'
                //P_Bad_Character_Rule
                P_ptr = P_Length - 2;

                while (P_ptr >= 0 && local_P[P_ptr] == local_P.Last())
                    P_ptr--;

                //P_ptr == -1, special case when string 'P' only consist of only one duplicated character
                if (P_ptr != -1) //string 'P' has character different from P.Last()
                {
                    P_Bad_Character_shift_table.Add(new P_Bad_Character_struct(local_P[P_ptr], local_P.Length - 1 - P_ptr));  //Add P second last character into the table
                    P_ptr--;

                    while (P_ptr >= 0)  //when string P.length >= 3
                    {
                        int i = 0;
                        while (i < P_Bad_Character_shift_table.Count() && P_Bad_Character_shift_table[i].P_Char != local_P[P_ptr])
                            i++;

                        if (i == P_Bad_Character_shift_table.Count() && local_P[P_ptr] != local_P.Last()) //P current character not occur inside P_Bad_Character_shift_table, add this new character & it position
                        {
                            P_Bad_Character_shift_table.Add(new P_Bad_Character_struct(local_P[P_ptr], local_P.Length - 1 - P_ptr));
                        }

                        P_ptr--;
                    }
                }
            }

            public List<int> string_search(string P, string T)
            {
                List<int> local_P_in_T_results = new List<int>();

                int start_P_in_T = 0;
                int end_P_in_T = P_Length - 1;
                int T_ptr;

                //Check string 'P' & 'T' are not empty and T string is longer than P string, or else definitely will not be any matches and no reason to proceed the checking
                if (!String.IsNullOrEmpty(P) && !String.IsNullOrEmpty(T) && T.Length >= P_Length)
                {
                    int P_ptr;  //Pointer showing position of target search string 'P' last character now
                    T_ptr = P_Length - 1;  //Pointer showing position of target search string 'P' last character in string 'T' now

                    int table_isEmpty = 0;

                    //Start string 'P' against string 'T' comparison
                    int matched_string_length = 0;
                    P_ptr = P_Length - 1;

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
                            switch (table_isEmpty)
                            {
                                case 1:
                                    //if (P_Bad_Character_shift_table.Count() == 0)

                                    //special case when string 'P' only consist of only one duplicated character
                                    start_P_in_T += P_Length;
                                    end_P_in_T += P_Length;
                                    break;

                                default:
                                    int i = 0;
                                    while (i < P_Bad_Character_shift_table.Count() && P_Bad_Character_shift_table[i].P_Char != T[end_P_in_T])
                                        i++;

                                    if (i == P_Bad_Character_shift_table.Count())
                                    {
                                        //string 'T' character correspond to string 'P' last character position cannot be found in string 'P'
                                        start_P_in_T += P_Length;
                                        end_P_in_T += P_Length;
                                    }
                                    else
                                    {
                                        start_P_in_T += P_Bad_Character_shift_table[i].Bad_Character_Shift;
                                        end_P_in_T += P_Bad_Character_shift_table[i].Bad_Character_Shift;
                                    }
                                    break;
                            }
                        }
                        else if (matched_string_length == 1)
                        {
                            //Only P last character match
                            start_P_in_T += only_last_character_of_P_matching_shift;
                            end_P_in_T += only_last_character_of_P_matching_shift;
                        }
                        else if (matched_string_length == P_Length) //found string 'P' matching string 'T' in this position
                        {
                            local_P_in_T_results.Add(start_P_in_T); //record the position of string 'P' in string 'T'
                            start_P_in_T += 1;
                            end_P_in_T += 1;
                        }
                        else  //P against T matched string is 2 characters or more but not exactly matched, only partially match
                        {
                            //Good Suffix Rule
                            start_P_in_T += P_Good_Suffix_shift_table[matched_string_length - 2];
                            end_P_in_T += P_Good_Suffix_shift_table[matched_string_length - 2];
                        }

                        T_ptr = end_P_in_T;
                        P_ptr = P_Length - 1;
                        matched_string_length = 0;

                        goto string_compare_P_against_T;
                    }
                }

                return local_P_in_T_results;
            }
        }
    }
}
