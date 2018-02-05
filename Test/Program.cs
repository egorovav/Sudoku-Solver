using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Test
{
	class Program
	{
		static void Main(string[] args)
		{
			double sum = 0;
			double cnt = 1000;
			for(int i = 1; i <= cnt; ++i)
			{
				sum += cnt / ((cnt + i) * (cnt + i));
			}

			Console.WriteLine(sum);
			Console.ReadLine();
		}
	}
}
