using System;
using System.Runtime.InteropServices;

class Program
{
    [DllImport("libuuid.so.1")]
    private static extern void uuid_generate(byte[] buffer);

    static void Main()
    {
        Console.WriteLine("Generating UUID using native glibc library...");

        try
        {
            var buffer = new byte[16];
            uuid_generate(buffer);
            Console.WriteLine("Success.");
        }
        catch (Exception ex)
        {
            Console.WriteLine("Exception:");
            Console.WriteLine(ex);
        }
    }
}
