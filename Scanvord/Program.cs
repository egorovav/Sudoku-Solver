using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Scanvord
{
	class Program
	{
		static void Main(string[] args)
		{

			int[] _grid = readGrid("input.txt");
			//int[] _grid = readHexGrid("input.txt");

			Console.WriteLine();
			var _start = new DateTime();
			var _end = new DateTime();
			var _result = new int[_grid.Length];
			int _turnCount = 10;
			double _sum = 0;
			for (int i = 0; i < _turnCount; i++)
			{

				_start = DateTime.Now;

				initialize(_grid);

				//_result = fastSolve(_grid, CellNums);
				//_result = getGuessCount(FlatCellNums);
				//_result = flatFastSolve(_grid, FlatCellNums);
				//writeGuessGridDiff();
				//_result = flatFastSolveMt(new PositionData() { Grid = _grid, CellNums = FlatCellNums, IsRunInNewThread = true });
				//_result = flatSolveLoopWide(_grid, FlatCellNums);
				_result = flatSolveLoopDeep2(_grid, FlatCellNums);

				_end = DateTime.Now;
				//writeGrid(_result, "output.txt");

				double _duration = (_end - _start).TotalMilliseconds;
				Console.WriteLine(_duration);
				_sum += _duration;
			}
			Console.WriteLine($"avg - {_sum / _turnCount}");
			writeGrid(_result, "output.txt");


			//ooSolve(_grid);
			//solve(_grid);

			//writeGrid(_result, "guess_count.txt");

			Console.ReadLine();
		}

		private static int[] getGridDifference(int[] grid1, int[] grid2)
		{
			var _result = new int[grid1.Length];
			for(int i = 0; i < grid1.Length; i++)
			{
				_result[i] = grid1[i] - grid2[i];
			}
			return _result;
		}

		private static void writeGuessGridDiff()
		{
			//int[] _grid1 = readGrid("input.txt");
			//initialize(_grid1);
			//int[] _guessCount1 = getGuessCount(FlatCellNums);

			int[] _grid2 = readGrid("input16eazy.txt");
			initialize(_grid2);
			int[] _guessCount2 = getGuessCount(FlatCellNums);

			int _count = 0;
			var _grid1 = new int[_grid2.Length];
			for (int i = 0; i < _grid2.Length; i++)
			{
				if (_grid2[i] != 0)
				{
					_grid2.CopyTo(_grid1, 0);
					_grid1[i] = 0;
					initialize(_grid1);
					int[] _guessCount1 = getGuessCount(FlatCellNums);
					var _diff = getGridDifference(_guessCount1, _guessCount2);
					if(_diff.Sum() == 11)
						writeGrid(_diff, "guess_count.txt");
					_count++;
				}
			}
			Console.WriteLine(_count);
		}

		private static void writeGrid(string fileName)
		{
			using (var _fs = new FileStream(fileName, FileMode.Create))
			{
				using (var _sw = new StreamWriter(_fs))
				{
					for (int i = 0; i < GridSize; i++)
					{
						for (int j = 0; j < GridSize; j++)
						{
							Console.Write($"{Rows[i][j]},");
							_sw.Write($"{Rows[i][j]},");
						}
						Console.WriteLine();
						_sw.WriteLine();
					}
				}
			}
		}

		private static void writeGrid(int[] aGrid, string fileName)
		{
			using (var _fs = new FileStream(fileName, FileMode.Append))
			{
				using (var _sw = new StreamWriter(_fs))
				{
					for (int i = 0; i < GridSize; i++)
					{
						for (int j = 0; j < GridSize; j++)
						{
							Console.Write($"{aGrid[i * GridSize + j]},");
							_sw.Write($"{aGrid[i * GridSize + j]},");
						}
						Console.WriteLine();
						_sw.WriteLine();
					}
					Console.WriteLine();
					_sw.WriteLine();
				}
			}
		}

		private static int[] getGuessCount(int[] cellNums)
		{
			var _result = new int[cellNums.Length];
			for (int i = 0; i < cellNums.Length; i++)
			{
				int[] _temp = bitNumbers(cellNums[i]);
				if (_temp != null)
				{
					_result[i] = _temp.Length;
				}
			}
			return _result;
		}

		private static void initialize(int[] grid)
		{
			Rows = new int[GridSize][];
			Columns = new int[GridSize][];
			Cells = new int[GridSize][];
			CellNums = new int[GridSize][][];
			Indexes = new int[GridSize][];
			Path = new Stack<int>();
			Remain = new List<int>();
			FlatCellNums = new int[GridSize * GridSize];
			int _allYes = (int)(Math.Pow(2, GridSize) - 1);

			for (int i = 0; i < GridSize; i++)
			{
				Rows[i] = new int[GridSize];
				Columns[i] = new int[GridSize];
				Cells[i] = new int[GridSize];
				CellNums[i] = new int[GridSize][];
				Indexes[i] = new int[GridSize];

				for (int j = 0; j < GridSize; j++)
				{
					CellNums[i][j] = new int[GridSize];
					for (int k = 0; k < GridSize; k++)
					{
						CellNums[i][j][k] = 1;
					}
					FlatCellNums[j * GridSize + i] = _allYes;
					Remain.Add(i * GridSize + j);
				}
			}

			int N = GridSize * GridSize;

			for (int k = 0; k < N; k++)
			{
				if (grid[k] != 0)
				{
					int x = k % GridSize;
					int y = k / GridSize;

					addValue(x, y, grid[k], CellNums);
					addFlatValue(k, grid[k] - 1, FlatCellNums);
					CellNums[x][y] = new int[GridSize];
				}
			}
		}

		private static int[][][] CellNums;
		private static int CellSize = 3;
		private static int GridSize = 9;
		private static int[][] Rows;
		private static int[][] Columns;
		private static int[][] Cells;
		private static int[][] Indexes;
		private static Stack<int> Path;
		private static List<int> Remain;
		private static int[] FlatCellNums;

		static bool setValue(int x, int y, int value, int[][] rows, int[][] columns, int[][] cells)
		{
			if (rows[y][x] == value)
				return true;

			if (value != 0 && Array.IndexOf(rows[y], value) >= 0)
				return false;

			if (value != 0 && Array.IndexOf<int>(columns[x], value) >= 0)
				return false;

			int N = (y / CellSize) * CellSize + (x / CellSize);
			int n = (y % CellSize) * CellSize + (x % CellSize);
			if (value != 0 && Array.IndexOf<int>(cells[N], value) >= 0)
				return false;

			rows[y][x] = value;
			columns[x][y] = value;
			cells[N][n] = value;

			return true;
		}

		static int[][][] Copy3dArray(int[][][] arr)
		{
			var _result = new int[arr.Length][][];
			for(int i = 0; i < arr.Length; i++)
			{
				_result[i] = new int[arr[i].Length][];
				for (int j = 0; j < arr.Length; j++)
				{
					_result[i][j] = new int[arr[i][j].Length];
					arr[i][j].CopyTo(_result[i][j], 0);
				}
			}

			return _result;
		}

		static bool setValue(int x, int y, int value)
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

		static void removeValue(int x, int y, int value)
		{
			for(int i = 0; i < GridSize; i++)
			{
				CellNums[x][i][value - 1] = 1;
				if(i != x)
					CellNums[i][y][value - 1] = 1;
			}

			for(int i = (x / CellSize) * CellSize; i < (x / CellSize) * CellSize + CellSize; i++)
			{
				for(int j = (y / CellSize) * CellSize; j < (y / CellSize) * CellSize + CellSize; j++)
				{
					if(i != x && j != y)
					{
						CellNums[i][j][value - 1] = 1;
					}
				}
			}
		}

		static void addValue(int x, int y, int value, int[][][] cellNums)
		{
			for (int i = 0; i < GridSize; i++)
			{
				cellNums[x][i][value - 1] = 0;
				if (i != x)
					cellNums[i][y][value - 1] = 0;
			}

			for (int i = (x / CellSize) * CellSize; i < (x / CellSize) * CellSize + CellSize; i++)
			{
				for (int j = (y / CellSize) * CellSize; j < (y / CellSize) * CellSize + CellSize; j++)
				{
					if (i != x && j != y)
					{
						cellNums[i][j][value - 1] = 0;
					}
				}
			}
		}

		static void addFlatValue(int index, int value, int[] flatCellNums, int startIndex)
		{
			int x = index % GridSize;
			int y = index / GridSize;
			int _mask = ~(1 << value);
			for(int i = 0; i < GridSize; i++)
			{
				flatCellNums[i * GridSize + x + startIndex] &= _mask;
				flatCellNums[y * GridSize + i + startIndex] &= _mask;
			}

			for (int i = (x / CellSize) * CellSize; i < (x / CellSize) * CellSize + CellSize; i++)
			{
				for (int j = (y / CellSize) * CellSize; j < (y / CellSize) * CellSize + CellSize; j++)
				{
					flatCellNums[j * GridSize + i + startIndex] &= _mask;
				}
			}

			flatCellNums[index + startIndex] = 0;
		}

		static void addFlatValue(int index, int value, int[] flatCellNums)
		{
			int x = index % GridSize;
			int y = index / GridSize;
			int _mask = ~(1 << value);
			for (int i = 0; i < GridSize; i++)
			{
				flatCellNums[i * GridSize + x] &= _mask;
				flatCellNums[y * GridSize + i] &= _mask;
			}

			for (int i = (x / CellSize) * CellSize; i < (x / CellSize) * CellSize + CellSize; i++)
			{
				for (int j = (y / CellSize) * CellSize; j < (y / CellSize) * CellSize + CellSize; j++)
				{
					flatCellNums[j * GridSize + i] &= _mask;
				}
			}

			flatCellNums[index] = 0;
		}

		static void addFlatValue(int x, int y, int value, int[] flatCellNums)
		{
			int _mask = ~(1 << value);
			for (int i = 0; i < GridSize; i++)
			{
				flatCellNums[i * GridSize + x] &= _mask;
				if (i != x)
					flatCellNums[y * GridSize + i] &= _mask;
			}

			for (int i = (x / CellSize) * CellSize; i < (x / CellSize) * CellSize + CellSize; i++)
			{
				for (int j = (y / CellSize) * CellSize; j < (y / CellSize) * CellSize + CellSize; j++)
				{
					if (i != x && j != y)
					{
						flatCellNums[j * GridSize + i] &= _mask;
					}
				}
			}
		}

		static int[] readHexGrid(string fileName)
		{
			GridSize = 0;
			int[] _result = null;
			using (var _fs = new FileStream(fileName, FileMode.Open))
			{
				using (var _sr = new StreamReader(_fs))
				{
					string _str = null;
					int _rowIndex = 0;
					while ((_str = _sr.ReadLine()) != null)
					{
						if (String.IsNullOrEmpty(_str))
							continue;

						_str.Replace(" ", "");
						var _numbers = _str.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);

						if (GridSize == 0)
						{
							var _cellSize = Math.Sqrt(_numbers.Length);
							if (Math.Abs(Math.Round(_cellSize) - _cellSize) > Double.Epsilon)
								throw new ArgumentException("Incorrect suddoku initial postion. The table is not square.");

							GridSize = _numbers.Length;
							CellSize = (int)Math.Round(_cellSize);
							_result = new int[GridSize * GridSize];
						}
						else
						{
							if (GridSize != _numbers.Length)
								throw new ArgumentException("Incorrect suddoku initial postion. The table is not square.");
						}

						for (int i = 0; i < GridSize; i++)
						{
							int _value = 0;
							if (Int32.TryParse(_numbers[i], out _value))
							{
								_value++;
							}
							else
							{
								switch (_numbers[i])
								{
									case "*": _value = 0; break;
									case "A": _value = 11; break;
									case "B": _value = 12; break;
									case "C": _value = 13; break;
									case "D": _value = 14; break;
									case "E": _value = 15; break;
									case "F": _value = 16; break;
									default:
										throw new ArgumentException("Incorrect suddoku initial postion. A char is not digit.");
								}
							}
								

							_result[_rowIndex * GridSize + i] = _value;
						}

						_rowIndex++;
					}
				}

			}
			return _result;
		}

		static int[] readGrid(string fileName)
		{
			GridSize = 0;
			int[] _result = null;
			using (var _fs = new FileStream(fileName, FileMode.Open))
			{
				using (var _sr = new StreamReader(_fs))
				{
					string _str = null;
					int _rowIndex = 0;
					while((_str = _sr.ReadLine()) != null)
					{
						_str.Replace(" ", "");
						var _numbers = _str.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);

						if (GridSize == 0)
						{
							var _cellSize = Math.Sqrt(_numbers.Length);
							if (Math.Abs(Math.Round(_cellSize) - _cellSize) > Double.Epsilon)
								throw new ArgumentException("Incorrect suddoku initial postion. The table is not square.");

							GridSize = _numbers.Length;
							CellSize = (int)Math.Round(_cellSize);
							_result = new int[GridSize * GridSize];
						}
						else
						{
							if(GridSize != _numbers.Length)
								throw new ArgumentException("Incorrect suddoku initial postion. The table is not square.");
						}

						for (int i = 0; i < GridSize; i++)
						{
							int _value = 0;
							if(!Int32.TryParse(_numbers[i], out _value))
								throw new ArgumentException("Incorrect suddoku initial postion. A char is not digit.");

							_result[_rowIndex * GridSize + i] = _value;
						}

						_rowIndex++;
					}
				}

			}
			return _result;
		}

		static void ooSolve(int[] grid)
		{
			if (Array.IndexOf(grid, 0) == -1)
				return;

			var _position = new Position(grid);
			if(Counter > 2000)
			{
				_position.solve(grid);
				return;
			}

			int _minCellNum = GridSize + 1;
			int _minCellNumX = -1;
			int _minCellNumY = -1;
			for (int x = 0; x < GridSize; x++)
			{
				for (int y = 0; y < GridSize; y++)
				{
					if (_position.CellNums[x][y].Count != 0 && _position.CellNums[x][y].Count < _minCellNum)
					{
						_minCellNum = _position.CellNums[x][y].Count;
						_minCellNumX = x;
						_minCellNumY = y;
					}
				}
			}

			if (_minCellNumX >= 0)
			{
				for (int s = 0; s < _minCellNum; s++)
				{
					int value = _position.CellNums[_minCellNumX][_minCellNumY][s];
					if (_position.setValue(_minCellNumX, _minCellNumY, value))
					{
						grid[_minCellNumY * GridSize + _minCellNumX] = value;
						Counter++;
						ooSolve(grid);
					}
				}
			}
		}

		static int Counter = 0;

		static int bitNumbers(int n, int[] nums)
		{
			if (n == 0)
				return 0;

			int _index = 0;

			for(int i = 0; i < GridSize; i++)
			{
				if ((n & (1 << i)) != 0)
				{
					nums[_index] = i;
					_index++;
				}
			}

			return _index;
		}

		static int[] bitNumbers(int n)
		{
			if (n == 0)
				return null;

			int _index = 0;
			int[] _temp = new int[GridSize];

			for (int i = 0; i < GridSize; i++)
			{
				if ((n & (1 << i)) != 0)
				{
					_temp[_index] = i;
					_index++;
				}
			}

			int[] _result = new int[_index];
			Array.Copy(_temp, _result, _index);
			return _result;
		}

		static int[] flatSolveLoopDeep2(int[] grid, int[] guesses)
		{
			int _zeroCount = 0;
			for(int i = 0; i < grid.Length; i++)
			{
				if (grid[i] == 0)
					_zeroCount++;
			}

			int _stackDataSize = grid.Length * grid.Length;
			int _smallStackDataSize = grid.Length * GridSize;

			var _gridStack = new int[_stackDataSize];
			var _guessStack = new int[_stackDataSize];
			var _minGuessCellIndexStack = new int[grid.Length];
			var _minCellGuessesStack = new int[_smallStackDataSize];
			var _guessesCountStack = new int[grid.Length];
			var _curIndexStack = new int[grid.Length];
			int _stackIndex = 0;

			int _minGuessCount = GridSize + 1;
			int _minGuessCellIndex = -1;
			int[] _cellGuesses = new int[GridSize];

			for (int i = 0; i < grid.Length; i++)
			{
				int _guessCount = bitNumbers(guesses[i], _cellGuesses);
				if (_guessCount > 0 && _guessCount < _minGuessCount)
				{
					_minGuessCount = _guessCount;
					_minGuessCellIndex = i;
					Array.Copy(_cellGuesses, 0, _minCellGuessesStack, _stackIndex * GridSize, GridSize);
				}
			}

			Array.Copy(grid, 0, _gridStack, _stackIndex * grid.Length, grid.Length);
			Array.Copy(guesses, 0, _guessStack, _stackIndex * grid.Length, grid.Length);
			_minGuessCellIndexStack[_stackIndex] = _minGuessCellIndex;
			_guessesCountStack[_stackIndex] = _minGuessCount;
			_curIndexStack[_stackIndex] = -1;

			while (_stackIndex >= 0)
			{
				//int[] _gridLeaf = _gridStack[_stackIndex];
				//int[] _guessLeaf = _guessStack[_stackIndex];
				_minGuessCellIndex = _minGuessCellIndexStack[_stackIndex];
				//_minCellGuesses = _minCellGuessesStack[_stackIndex];
				int _curIndex = _curIndexStack[_stackIndex];
				_curIndex++;

				if (_curIndex == _guessesCountStack[_stackIndex])
				{
					_stackIndex--;
					continue;
				}
				else
				{
					_curIndexStack[_stackIndex] = _curIndex;
				}


				while (_minGuessCellIndex >= 0)
				{
					int _startIndex = _stackIndex * grid.Length;
					int _nextStartIndex = _startIndex + grid.Length;
					int _value = _minCellGuessesStack[_stackIndex * GridSize + _curIndex];

					Array.Copy(_gridStack, _startIndex, _gridStack, _nextStartIndex, grid.Length);
					_gridStack[_nextStartIndex + _minGuessCellIndex] = _value + 1;

					if (_stackIndex == _zeroCount - 1)
					{
						var _result = new int[grid.Length];
						Array.Copy(_gridStack, _nextStartIndex, _result, 0, grid.Length);
						return _result;
					}

					Array.Copy(_guessStack, _startIndex, _guessStack, _nextStartIndex, grid.Length);
					addFlatValue(_minGuessCellIndex, _value, _guessStack, _nextStartIndex);

					_minGuessCount = GridSize + 1;
					_minGuessCellIndex = -1;

					for (int i = 0; i < grid.Length; i++)
					{
						int _guessCount = bitNumbers(_guessStack[_nextStartIndex + i], _cellGuesses);

						if (_guessCount > 0 && _guessCount < _minGuessCount)
						{
							_minGuessCount = _guessCount;
							_minGuessCellIndex = i;
							Array.Copy(_cellGuesses, 0, _minCellGuessesStack, (_stackIndex + 1) * GridSize, GridSize);
						}
					}

					if (_minGuessCellIndex >= 0)
					{
						_stackIndex++;
						_minGuessCellIndexStack[_stackIndex] = _minGuessCellIndex;
						_guessesCountStack[_stackIndex] = _minGuessCount;

						_curIndex = 0;
						_curIndexStack[_stackIndex] = _curIndex;
					}
				}
			}
			return new int [grid.Length];
		}

		static int[] flatSolveLoopDeep1(int[] grid, int[] guesses)
		{
			var _gridStack = new int[grid.Length][];
			var _guessStack = new int[grid.Length][];
			var _minGuessCellIndexStack = new int[grid.Length];
			var _minCellGuessesStack = new int[grid.Length][];
			var _curIndexStack = new int[grid.Length];
			int _stackIndex = -1;

			int _minGuessCount = GridSize + 1;
			int _minGuessCellIndex = -1;
			int[] _minCellGuesses = null;

			for (int i = 0; i < grid.Length; i++)
			{
				int[] _cellGuesses = bitNumbers(guesses[i]);
				if (_cellGuesses != null)
				{
					int _guessCount = _cellGuesses.Length;
					if (_guessCount < _minGuessCount)
					{
						_minGuessCount = _guessCount;
						_minGuessCellIndex = i;
						_minCellGuesses = _cellGuesses;
					}
				}
			}

			_stackIndex++;
			_gridStack[_stackIndex] = grid;
			_guessStack[_stackIndex] = guesses;
			_minGuessCellIndexStack[_stackIndex] = _minGuessCellIndex;
			_minCellGuessesStack[_stackIndex] = _minCellGuesses;
			_curIndexStack[_stackIndex] = -1;

			while (true)
			{
				int[] _gridLeaf = _gridStack[_stackIndex];
				int[] _guessLeaf = _guessStack[_stackIndex];
				_minGuessCellIndex = _minGuessCellIndexStack[_stackIndex];
				_minCellGuesses = _minCellGuessesStack[_stackIndex];
				int _curIndex = _curIndexStack[_stackIndex];
				_curIndex++;

				if (_curIndex == _minCellGuesses.Length)
				{
					_stackIndex--;
					continue;
				}
				else
				{
					_curIndexStack[_stackIndex] = _curIndex;
				}


				while (_minGuessCellIndex >= 0)
				{
					var _g = new int[grid.Length];
					_gridLeaf.CopyTo(_g, 0);
					_g[_minGuessCellIndex] = _minCellGuesses[_curIndex] + 1;

					if (Array.IndexOf(_g, 0) < 0)
						return _g;

					var _c = new int[grid.Length];
					_guessLeaf.CopyTo(_c, 0);
					addFlatValue(_minGuessCellIndex, _minCellGuesses[_curIndex], _c);

					_minGuessCount = GridSize + 1;
					_minGuessCellIndex = -1;
					_minCellGuesses = null;

					for (int i = 0; i < grid.Length; i++)
					{
						int[] _cellGuesses = bitNumbers(_c[i]);
						if (_cellGuesses != null)
						{
							int _guessCount = _cellGuesses.Length;
							if (_guessCount < _minGuessCount)
							{
								_minGuessCount = _guessCount;
								_minGuessCellIndex = i;
								_minCellGuesses = _cellGuesses;
							}
						}
					}

					if (_minGuessCellIndex >= 0)
					{
						_stackIndex++;
						_gridStack[_stackIndex] = _g;
						_guessStack[_stackIndex] = _c;
						_minGuessCellIndexStack[_stackIndex] = _minGuessCellIndex;
						_minCellGuessesStack[_stackIndex] = _minCellGuesses;
						_curIndexStack[_stackIndex] = 0;

						_curIndex = 0;
						_gridLeaf = _g;
						_guessLeaf = _c;
					}
				}
			}
		}

		static int[] flatSolveLoopDeep(int[] grid, int[] guesses)
		{
			var _gridStack = new Stack<int[]>();
			var _guessStack = new Stack<int[]>();
			var _minGuessCellIndexStack = new Stack<int>();
			var _minCellGuessesStack = new Stack<int[]>();
			var _curIndexStack = new Stack<int>();

			int _minGuessCount = GridSize + 1;
			int _minGuessCellIndex = -1;
			int[] _minCellGuesses = null;

			for (int i = 0; i < grid.Length; i++)
			{
				int[] _cellGuesses = bitNumbers(guesses[i]);
				if (_cellGuesses != null)
				{
					int _guessCount = _cellGuesses.Length;
					if (_guessCount < _minGuessCount)
					{
						_minGuessCount = _guessCount;
						_minGuessCellIndex = i;
						_minCellGuesses = _cellGuesses;
					}
				}
			}

			_gridStack.Push(grid);
			_guessStack.Push(guesses);
			_minGuessCellIndexStack.Push(_minGuessCellIndex);
			_minCellGuessesStack.Push(_minCellGuesses);
			_curIndexStack.Push(-1);

			while (true)
			{
				int[] _gridLeaf = _gridStack.Peek();
				int[] _guessLeaf = _guessStack.Peek();
				_minGuessCellIndex = _minGuessCellIndexStack.Peek();
				_minCellGuesses = _minCellGuessesStack.Peek();
				int _curIndex = _curIndexStack.Pop();
				_curIndex++;

				if (_curIndex == _minCellGuesses.Length)
				{
					_gridStack.Pop();
					_guessStack.Pop();
					_minGuessCellIndexStack.Pop();
					_minCellGuessesStack.Pop();
					continue;
				}
				else
				{
					_curIndexStack.Push(_curIndex);
				}
			

				while (_minGuessCellIndex >= 0)
				{
					var _g = new int[grid.Length];
					_gridLeaf.CopyTo(_g, 0);
					_g[_minGuessCellIndex] = _minCellGuesses[_curIndex] + 1;

					if (Array.IndexOf(_g, 0) < 0)
						return _g;

					var _c = new int[grid.Length];
					_guessLeaf.CopyTo(_c, 0);
					addFlatValue(_minGuessCellIndex, _minCellGuesses[_curIndex], _c);

					_minGuessCount = GridSize + 1;
					_minGuessCellIndex = -1;
					_minCellGuesses = null;

					for (int i = 0; i < grid.Length; i++)
					{
						int[] _cellGuesses = bitNumbers(_c[i]);
						if (_cellGuesses != null)
						{
							int _guessCount = _cellGuesses.Length;
							if (_guessCount < _minGuessCount)
							{
								_minGuessCount = _guessCount;
								_minGuessCellIndex = i;
								_minCellGuesses = _cellGuesses;
							}
						}
					}

					if (_minGuessCellIndex >= 0)
					{
						_gridStack.Push(_g);
						_guessStack.Push(_c);
						_minGuessCellIndexStack.Push(_minGuessCellIndex);
						_minCellGuessesStack.Push(_minCellGuesses);
						_curIndexStack.Push(0);

						_curIndex = 0;
						_gridLeaf = _g;
						_guessLeaf = _c;
					}
				}
			}
		}

		static int[] flatSolveLoopWide(int[] grid, int[] flatCellNums)
		{
			var _grids = new int[1][];
			_grids[0] = grid;
			var _newGrids = new int[16][];
			var _guesses = new int[1][];
			_guesses[0] = flatCellNums;
			var _newGuesses = new int[16][];

			while(true)
			{
				int _gridsCount = 0;
				//Console.WriteLine(_gridsCount);
				int _newGridsIndex = 0;
				int _newGuessesIndex = 0;
				for(int _leafNumber = 0; _leafNumber < _grids.Length; _leafNumber++)
				{
					var _guessLeaf = _guesses[_leafNumber];
					if (_guessLeaf == null)
					{
						break;
					}
					else
					{
						_gridsCount++;
					}

					var _gridLeaf = _grids[_leafNumber];

					int _minGuessCount = GridSize + 1;
					int _minGuessCellIndex = -1;
					int[] _minCellGuesses = null;

					for (int i = 0; i < grid.Length; i++)
					{
						int[] _cellGuesses = bitNumbers(_guessLeaf[i]);
						if (_cellGuesses != null)
						{
							int _guessCount = _cellGuesses.Length;
							if (_guessCount < _minGuessCount)
							{
								_minGuessCount = _guessCount;
								_minGuessCellIndex = i;
								_minCellGuesses = _cellGuesses;
							}
						}
					}
					

					if (_minGuessCellIndex < 0)
						continue;

					for (int i = 0; i < _minGuessCount; i++)
					{
						var _g = new int[grid.Length];
						Array.Copy(_gridLeaf, _g, grid.Length);
						_g[_minGuessCellIndex] = _minCellGuesses[i] + 1;
						if (Array.IndexOf(_g, 0) < 0)
							return _g;

						var _c = new int[flatCellNums.Length];
						Array.Copy(_guessLeaf, _c, flatCellNums.Length);
						addFlatValue(_minGuessCellIndex, _minCellGuesses[i], _c);

						_newGrids[_newGridsIndex] = _g;
						_newGridsIndex++;
						_newGuesses[_newGuessesIndex] = _c;
						_newGuessesIndex++;
					}			
				}

				_grids = _newGrids;
				_guesses = _newGuesses;
				_newGrids = new int[_gridsCount * 16][];
				_newGuesses = new int[_gridsCount * 16][];
			}
		}

		static int[] flatFastSolveMt(object  oData)
		{
			var data = (PositionData)oData;

			int _minGuessCount = GridSize + 1;
			int _minCellX = -1;
			int _minCellY = -1;
			for (int i = 0; i < GridSize; i++)
			{
				for (int j = 0; j < GridSize; j++)
				{
					int _guessCount = 0;
					int[] _temp = bitNumbers(data.CellNums[j * GridSize + i]);
					if (_temp.Length != 0)
					{
						_guessCount = _temp.Length;
					}
					
					if (_guessCount != 0 && _guessCount < _minGuessCount)
					{
						_minGuessCount = _guessCount;
						_minCellX = i;
						_minCellY = j;
					}
				}
			}

			if (_minGuessCount == GridSize + 1)
			{
				return data.Grid;
			}

			var _result = data.Grid;
			List<Task<int[]>> _tasks = null;

			for (int k = 0; k < GridSize; k++)
			{
				if ((data.CellNums[_minCellY * GridSize + _minCellX] & (1 << k)) != 0)
				{
					var _grid = new int[data.Grid.Length];
					Array.Copy(data.Grid, _grid, data.Grid.Length);
					var _cellNums = new int[data.CellNums.Length];
					Array.Copy(data.CellNums, _cellNums, data.CellNums.Length);

					_grid[_minCellY * GridSize + _minCellX] = k + 1;
					addFlatValue(_minCellY * GridSize + _minCellX, k, _cellNums);
					var _data = new PositionData();
					_data.Grid = _grid;
					_data.CellNums = _cellNums;

					if (data.IsRunInNewThread && _minGuessCount > 1)
					{
						_data.IsRunInNewThread = false;
						Func<object, int[]> _f = new Func<object, int[]>(flatFastSolveMt);
						Task<int[]> _task = new Task<int[]>(_f, _data);
						if (_tasks == null)
							_tasks = new List<Task<int[]>>();
						_tasks.Add(_task);
						_task.Start();
					}
					else
					{
						_data.IsRunInNewThread = data.IsRunInNewThread;
						_result = flatFastSolveMt(_data);
					}
					if (Array.IndexOf(_result, 0) < 0)
						break;
				}
			}

			if (_tasks != null)
			{
				Task.WaitAll(_tasks.ToArray());
				foreach(var _task in _tasks)
				{
					_result = _task.Result;
					if (Array.IndexOf(_result, 0) < 0)
						break;
				}
			}

			return _result;
		}

		static int[] flatFastSolve(int[] grid, int[] flatCellNums)
		{
			int _minGuessCount = GridSize + 1;
			int _minCellIndex = -1;
			int[] _minCellGuesses = null;
			for (int i = 0; i < grid.Length; i++)
			{
				int _guessCount = 0;
				int[] _temp = bitNumbers(flatCellNums[i]);
				if(_temp != null)
				{
					_guessCount = _temp.Length;
				}

				if (_guessCount != 0 && _guessCount < _minGuessCount)
				{
					_minGuessCount = _guessCount;
					_minCellIndex = i;
					_minCellGuesses = _temp;
				}
			}

			if (_minGuessCount == GridSize + 1)
			{
				return grid;
			}

			var _result = grid;

			for (int k = 0; k < _minGuessCount; k++)
			{
				var _grid = new int[grid.Length];
				Array.Copy(grid, _grid, grid.Length);
				var _cellNums = new int[flatCellNums.Length];
				Array.Copy(flatCellNums, _cellNums, flatCellNums.Length);

				_grid[_minCellIndex] = _minCellGuesses[k] + 1;
				addFlatValue(_minCellIndex, _minCellGuesses[k], _cellNums);
				_result = flatFastSolve(_grid, _cellNums);
				if (Array.IndexOf(_result, 0) < 0)
					break;
			}

			return _result;
		}

		static int[] flatFastSolve1(int[] grid, int[] flatCellNums)
		{
			int _minGuessCount = GridSize + 1;
			int _minCellX = -1;
			int _minCellY = -1;
			for (int i = 0; i < GridSize; i++)
			{
				for (int j = 0; j < GridSize; j++)
				{
					int _guessCount = 0;
					int[] _temp = bitNumbers(flatCellNums[j * GridSize + i]);
					if (_temp != null)
					{
						_guessCount = _temp.Length;
					}

					if (_guessCount != 0 && _guessCount < _minGuessCount)
					{
						_minGuessCount = _guessCount;
						_minCellX = i;
						_minCellY = j;
					}
				}
			}

			if (_minGuessCount == GridSize + 1)
			{
				return grid;
			}

			var _result = grid;
			var _index = _minCellY * GridSize + _minCellX;
			for (int k = 0; k < GridSize; k++)
			{
				if ((flatCellNums[_index] & (1 << k)) != 0)
				{
					var _grid = new int[grid.Length];
					Array.Copy(grid, _grid, grid.Length);
					var _cellNums = new int[flatCellNums.Length];
					Array.Copy(flatCellNums, _cellNums, flatCellNums.Length);

					_grid[_index] = k + 1;
					addFlatValue(_minCellX, _minCellY, k, _cellNums);
					_cellNums[_index] = 0;
					_result = flatFastSolve(_grid, _cellNums);
					if (Array.IndexOf(_result, 0) < 0)
						break;
				}
			}

			return _result;
		}

		static int[] fastSolve(int[] grid, int[][][] cellNums)
		{

				int _minGuessCount = GridSize + 1;
				int _minCellX = -1;
				int _minCellY = -1;
				for (int i = 0; i < GridSize; i++)
				{
					for (int j = 0; j < GridSize; j++)
					{
						int _guessCount = cellNums[i][j].Sum();
						if (_guessCount != 0 && _guessCount < _minGuessCount)
						{
							_minGuessCount = _guessCount;
							_minCellX = i;
							_minCellY = j;
						}
					}
				}

				if (_minGuessCount == GridSize + 1)
				{
					return grid;
				}

			var _grid = new int[grid.Length];
			Array.Copy(grid, _grid, grid.Length);
			var _cellNums = CopyCellNums(cellNums);

			for (int k = 0; k < GridSize; k++)
			{
				if (cellNums[_minCellX][_minCellY][k] > 0)
				{
					_grid[_minCellY * GridSize + _minCellX] = k + 1;
					addValue(_minCellX, _minCellY, k + 1, _cellNums);
					_cellNums[_minCellX][_minCellY] = new int[GridSize];
					grid = fastSolve(_grid, _cellNums);
				}
			}

			return grid;
		}

		static int[][][] CopyCellNums(int[][][] cellNums)
		{
			return Copy3dArray(cellNums);
		}

		static void solve(int[] grid)
		{
			int N = GridSize * GridSize;

			//writeGrid();

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
				if(grid[i] != 0)
				{
					i++;
					continue;
				}

				int _value = Rows[y][x];
				bool _isValueFounded = false;
				for (int j = _value + 1; j <= GridSize; j++)
				{
					if(setValue(x, y, j))
					{
						_isValueFounded = true;
						break;
					}

				}

				if (_isValueFounded)
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
