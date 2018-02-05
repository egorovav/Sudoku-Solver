using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Scanvord
{
	public class Position
	{

		public Position(int[] grid)
		{
			initialize(grid);
		}

		private void initialize(int[] grid)
		{
			GridSize = (int)Math.Round(Math.Sqrt(grid.Length));
			CellSize = (int)Math.Round(Math.Sqrt(GridSize));

			Rows = new int[GridSize][];
			Columns = new int[GridSize][];
			Cells = new int[GridSize][];
			FCellNums = new List<int>[GridSize][];

			for (int i = 0; i < GridSize; i++)
			{
				Rows[i] = new int[GridSize];
				Columns[i] = new int[GridSize];
				Cells[i] = new int[GridSize];
				FCellNums[i] = new List<int>[GridSize];

				for (int j = 0; j < GridSize; j++)
				{
					FCellNums[i][j] = new List<int>(GridSize);
					for (int k = 1; k <= GridSize; k++)
					{
						FCellNums[i][j].Add(k);
					}
				}
			}

			for (int k = 0; k < grid.Length; k++)
			{
				if (grid[k] != 0)
				{
					int x = k % GridSize;
					int y = k / GridSize;
					if (setValue(x, y, grid[k]))
					{
						addValue(x, y, grid[k]);
					}
				}
			}
		}

		private List<int>[][] FCellNums;
		private int CellSize = 3;
		private int GridSize = 9;
		private int[][] Rows;
		private int[][] Columns;
		private int[][] Cells;

		public List<int>[][] CellNums
		{
			get { return this.FCellNums; }
		}

		public void addValue(int x, int y, int value)
		{
			for (int i = 0; i < GridSize; i++)
			{
				FCellNums[x][i].Remove(value);
				if (i != x)
					FCellNums[i][y].Remove(value);
			}

			for (int i = (x / CellSize) * CellSize; i < (x / CellSize) * CellSize + CellSize; i++)
			{
				for (int j = (y / CellSize) * CellSize; j < (y / CellSize) * CellSize + CellSize; j++)
				{
					if (i != x && j != y)
					{
						FCellNums[i][j].Remove(value);
					}
				}
			}
		}

		public bool setValue(int x, int y, int value)
		{
			if (Rows[y][x] == value)
				return true;

			if (value != 0 && Array.IndexOf(Rows[y], value) >= 0)
				return false;

			if (value != 0 && Array.IndexOf<int>(Columns[x], value) >= 0)
				return false;

			int N = (y / CellSize) * CellSize + (x / CellSize);
			int n = (y % CellSize) * CellSize + (x % CellSize);
			if (value != 0 && Array.IndexOf<int>(Cells[N], value) >= 0)
				return false;

			Rows[y][x] = value;
			Columns[x][y] = value;
			Cells[N][n] = value;

			return true;
		}

		public void solve(int[] grid)
		{
			int N = GridSize * GridSize;

			for (int k = 0; k < N; k++)
			{
				if (grid[k] != 0)
				{
					int x = k % GridSize;
					int y = k / GridSize;
					if (!setValue(x, y, grid[k]))
						throw new ArgumentException("Incorrect suddoku initial postion. Dublicate digits exists.");
				}
			}

			int i = 0;
			while (i < N)
			{
				int x = i % GridSize;
				int y = i / GridSize;
				if (grid[i] != 0)
				{
					i++;
					continue;
				}

				int _value = Rows[y][x];
				bool _isValueFinded = false;
				for (int j = _value + 1; j <= GridSize; j++)
				{
					if (setValue(x, y, j))
					{
						_isValueFinded = true;
						break;
					}

				}

				if (_isValueFinded)
				{
					do
					{
						i++;
					}
					while (i < N && grid[i] != 0);
				}
				else
				{
					setValue(x, y, 0);
					do
					{
						i--;
					}
					while (grid[i] != 0);
				}
			}
		}
	}
}
