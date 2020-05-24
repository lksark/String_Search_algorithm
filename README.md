# Fast Exact String Search algorithm

To search a substring within a long string in online mode, we can use Naive Exact Matching algorithm, Boyer Moore search algorithm and other algorithms.

In Naive Exact Matching search algorithm, every character in the string will be compared to find the matches from left most character in the string to the right. When found miss-match character, shift the target search string 1 character to the right of source string and continue the comparison.

Boyer Moore search algorithm is good tool to search string within a long string. It theoretically faster than Naive Exact Matching method. Boyer Moore search algorithm bypass a number of characters in a long string without comparison depend on the conditions.

The coding here are fast exact string search algorithm, bypass a number of characters in a long string without comparison.

Coding attached here are written in Microsoft C#.
