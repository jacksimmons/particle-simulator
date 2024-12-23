using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ParticleGame;

public static class Extensions
{
    //https://stackoverflow.com/questions/642542/how-to-get-next-or-previous-enum-value-in-c-sharp
    public static T Next<T>(this T src) where T : struct
    {
        if (!typeof(T).IsEnum) throw new ArgumentException(String.Format("Argument {0} is not an Enum", typeof(T).FullName));

        T[] Arr = (T[])Enum.GetValues(src.GetType());
        int next = Array.IndexOf<T>(Arr, src) + 1;
        return (Arr.Length == next) ? Arr[0] : Arr[next];
    }

    public static T Prev<T>(this T src) where T : struct
    {
        if (!typeof(T).IsEnum) throw new ArgumentException(String.Format("Argument {0} is not an Enum", typeof(T).FullName));

        T[] Arr = (T[])Enum.GetValues(src.GetType());
        int prev = Array.IndexOf<T>(Arr, src) - 1;
        return (-1 == prev) ? Arr[Arr.Length - 1] : Arr[prev];
    }

    public static void ChangeIndex(ref int index, int length, int increment)
    {
        index += increment;
        if (index < 0)
            index = length - 1;
        else if (index > length - 1)
            index = 0;
    }
}