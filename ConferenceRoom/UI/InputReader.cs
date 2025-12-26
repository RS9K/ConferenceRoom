using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Globalization;

namespace ConferenceRoom.UI;

public class InputReader
{
    public string ReadString(string prompt)
    {
        while (true)
        {
            Console.Write(prompt);
            var input = Console.ReadLine();
            if (!string.IsNullOrWhiteSpace(input))
                return input.Trim();
        }
    }

    public DateTime ReadDateTime(string prompt)
    {
        while (true)
        {
            Console.Write(prompt);
            var input = Console.ReadLine();

            if (DateTime.TryParseExact(
                input,
                "yyyy-MM-dd HH:mm",
                CultureInfo.InvariantCulture,
                DateTimeStyles.None,
                out var dt))
                return dt;

            Console.WriteLine("Invalid format. Use: yyyy-MM-dd HH:mm");
        }
    }
    public int ReadIntInRange(string prompt, int min, int max)
    {
        while (true)
        {
            Console.Write(prompt);
            var input = Console.ReadLine();

            if (int.TryParse(input, out var value) && value >= min && value <= max)
                return value;

            Console.WriteLine($"Invalid value. Enter a number between {min} and {max}.");
        }
    }
}

// ReadRequired(...)
// ReadLocalDateTime(...)