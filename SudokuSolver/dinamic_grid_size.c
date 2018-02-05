#include "dinamic_grid_size.h"

void initialize(int* guesses, int* grid, int grid_size)
{
	int cell_size = sqrt(grid_size);

	int all_yes = pow(2, grid_size) - 1;
	int grid_length = grid_size * grid_size;
	for (int i = 0; i < grid_length; i++)
	{
		guesses[i] = all_yes;
	}

	for (int i = 0; i < grid_length; i++)
	{
		add_value(i, grid[i] - 1, guesses, cell_size, grid_size);
	}
}

int* read_grid(char* fname, int* grid_size_loc)
{
	int grid_size = 0;
	FILE* f = fopen(fname, "r");
	if (!f)
	{
		printf("Cann't open file %s.", fname);
		return 0;
	}

	int* pg = 0;
	int row[100];
	int row_num = 0;

	while (!feof(f))
	{
		char str[100];
		if (!fgets(str, 100, f))
		{
			printf("File read error. %s", fname);
			return 0;
		}

		if (strlen(str) < 2)
			continue;

		int row_size = 0;

		char* tok = strtok(str, ", ");
		if (tok)
		{
			row[row_size] = atoi(tok);
			row_size++;
		}

		do
		{
			tok = strtok('\0', ", ");
			if (tok)
			{
				row[row_size] = atoi(tok);
				row_size++;
			}

		} while (tok);

		if (grid_size == 0)
		{
			grid_size = row_size;
			pg = /* (int*) */calloc(grid_size * grid_size, sizeof(int));
		}
		else
		{
			if (grid_size != row_size)
			{
				printf("Grid isn't rectangle. %s", fname);
				return 0;
			}
		}

		memcpy(pg + row_num * grid_size, row, grid_size * sizeof(int));

		if (row_size > 0)
			row_num++;
	}

	if (row_num != grid_size)
	{
		printf("Grid isn't rectangle. %s", fname);
		return 0;
	}

	*grid_size_loc = grid_size;

	return pg;
}

int* read_hex_grid(char* fname, int* grid_size_loc)
{
	int grid_size = 0;
	FILE* f = fopen(fname, "r");
	if (!f)
	{
		printf("Cann't open file %s.", fname);
		return 0;
	}

	int* pg;
	int row[100];
	int row_num = 0;
	while (!feof(f))
	{
		char str[100];
		if (!fgets(str, 100, f))
		{
			printf("File read error. %s", fname);
			return 0;
		}

		//puts(str);

		if (strlen(str) < 2)
			continue;

		int row_size = 0;
		int is_end_line_found = 0;
		for (int i = 0; i < 100; i++)
		{
			int c = str[i];

			if (c < 0 || c > 255)
			{
				is_end_line_found = 1;
				break;
			}

			if (isdigit(c))
			{
				row[row_size] = c + 1 - '0';
				row_size++;

			}
			else
			{
				if (isalnum(c))
				{
					row[row_size] = c + 11 - 'A';
					row_size++;
				}

				if (c == '*')
				{
					row[row_size] = 0;
					row_size++;
				}
			}
		}

		//printf("row size - %d\n", row_size);

		if (!is_end_line_found)
		{
			printf("File read error. %s", fname);
			return 0;
		}

		if (grid_size == 0)
		{
			grid_size = row_size;
			pg = /* (int*) */calloc(grid_size * grid_size, sizeof(int));
		}
		else
		{
			if (grid_size != row_size)
			{
				printf("Grid isn't rectangle. %s", fname);
				return 0;
			}
		}

		memcpy(pg + row_num * grid_size, row, grid_size * sizeof(int));
		//printf("row num - %d\n", row_num);

		if (row_size > 0)
			row_num++;
	}

	if (row_num != grid_size)
	{
		printf("Grid isn't rectangle. %s", fname);
		return 0;
	}

	*grid_size_loc = grid_size;

	return pg;
}

void add_value(int index, int value, int* guesses, int cell_size, int grid_size)
{
	if (value < 0)
		return;

	int x = index % grid_size;
	int y = index / grid_size;
	int mask = ~(1 << value);

	for (int i = 0; i < grid_size; i++)
	{
		guesses[i * grid_size + x] &= mask;
		guesses[y * grid_size + i] &= mask;
	}

	for (int i = (x / cell_size) * cell_size; i < (x / cell_size) * cell_size + cell_size; i++)
	{
		for (int j = (y / cell_size) * cell_size; j < (y / cell_size) * cell_size + cell_size; j++)
		{
			if (i != x && j != y)
			{
				guesses[j * grid_size + i] &= mask;
			}
		}
	}

	guesses[index] = 0;
}

int bit_numbers(int n, int grid_size, int* bits)
{
	if (n == 0)
		return 0;

	int index = 0;

	for (int i = 0; i < grid_size; i++)
	{
		if ((n & (1 << i)) != 0)
		{
			*(bits + index) = i;
			index++;
		}
	}

	return index;
}

int is_zero_exists(int* grid, int grid_length)
{
	for (int i = 0; i < grid_length; i++)
	{
		if (grid[i] == 0)
			return 1;
	}
	return 0;
}

int* solve(int* grid, int* guesses, int grid_size)
{
	int grid_length = grid_size * grid_size;
	int grid_data_length = grid_length * sizeof(int);
	int grid_data_size = grid_size * sizeof(int);

	int** grid_stack =					/* (int**)*/calloc(grid_length, sizeof(int*));
	int** guess_stack =					/* (int**)*/calloc(grid_length, sizeof(int*));
	int* min_guess_cell_index_stack =	/* (int*) */calloc(grid_length, sizeof(int));
	int** min_cell_guesses_stack =		/* (int**)*/calloc(grid_length, sizeof(int*));
	int* cur_index_stack =				/* (int*) */calloc(grid_length, sizeof(int));
	int* leaf_count_stack =				/* (int*) */calloc(grid_length, sizeof(int));

	if (!grid_stack || !guess_stack || !min_guess_cell_index_stack || !min_cell_guesses_stack || !cur_index_stack || !leaf_count_stack)
	{
		printf("Memory allocation error in function 'solve'.");
		return 0;
	}

	int stack_index = 0;

	int min_guess_count = grid_size + 1;
	int min_guess_cell_index = -1;
	int* min_cell_guesses = 0;

	for (int i = 0; i < grid_length; i++)
	{
		int* cell_guesses = /* (int*) */calloc(grid_size, sizeof(int));
		if (!cell_guesses)
		{
			printf("Memory allocation error in function 'solve'.");
			return 0;
		}

		int guess_count = bit_numbers(guesses[i], grid_size, cell_guesses);
		if (guess_count != 0 && guess_count < min_guess_count)
		{
			min_guess_count = guess_count;
			min_guess_cell_index = i;
			if (min_cell_guesses != 0)
				free(min_cell_guesses);
			min_cell_guesses = cell_guesses;
		}
		else
		{
			free(cell_guesses);
		}
	}

	if (min_cell_guesses == 0)
	{
		return grid;
	}

	int cell_size = sqrt(grid_size);

	grid_stack[stack_index] = grid;
	guess_stack[stack_index] = guesses;
	min_guess_cell_index_stack[stack_index] = min_guess_cell_index;
	min_cell_guesses_stack[stack_index] = min_cell_guesses;
	leaf_count_stack[stack_index] = min_guess_count;
	cur_index_stack[stack_index] = -1;

	while (1)
	{
		int* grid_leaf = grid_stack[stack_index];
		int* guess_leaf = guess_stack[stack_index];
		min_guess_cell_index = min_guess_cell_index_stack[stack_index];
		min_cell_guesses = min_cell_guesses_stack[stack_index];
		min_guess_count = leaf_count_stack[stack_index];
		int cur_index = cur_index_stack[stack_index];
		cur_index++;

		if (cur_index == min_guess_count)
		{
			//if (grid_stack[stack_index] != 0)
			//	free(grid_stack[stack_index]);

			//if (guess_stack[stack_index] != 0)
			//	free(guess_stack[stack_index]);

			//if (min_cell_guesses_stack[stack_index] != 0)
			//	free(min_cell_guesses_stack[stack_index]);

			free(grid_leaf);
			free(guess_leaf);
			free(min_cell_guesses);

			stack_index--;
			continue;
		}
		else
		{
			cur_index_stack[stack_index] = cur_index;
		}

		while (min_guess_cell_index >= 0)
		{

			//int* _g = /* (int*) */calloc(grid_length, sizeof(int));
			int* _g = /* (int*) */malloc(grid_data_length);
			if (!_g)
			{
				printf("Memory allocation error in function 'solve'.");
				return 0;
			}
			memcpy(_g, grid_leaf, grid_length * sizeof(int));
			_g[min_guess_cell_index] = min_cell_guesses[cur_index] + 1;

			if (!is_zero_exists(_g, grid_length))
			{
				// printf("stack index - %d\n", stack_index);
				for (int i = 0; i < grid_length; i++)
				{
					//printf("%d - %d\n", i, grid_stack[i]);
					if (grid_stack[i] != 0)
						free(grid_stack[i]);

					if (guess_stack[i] != 0)
						free(guess_stack[i]);

					if (min_cell_guesses_stack[i] != 0)
						free(min_cell_guesses_stack[i]);
				}

				free(grid_stack);
				free(guess_stack);
				free(min_guess_cell_index_stack);
				free(min_cell_guesses_stack);
				free(cur_index_stack);
				free(leaf_count_stack);
				return _g;
			}

			//int* _c = /* (int*) */ calloc(grid_length, sizeof(int));
			int* _c = malloc(grid_data_length);
			if (!_c)
			{
				printf("Memory allocation error in function 'solve'.");
				return 0;
			}
			memcpy(_c, guess_leaf, grid_length * sizeof(int));
			add_value(min_guess_cell_index, min_cell_guesses[cur_index], _c, cell_size, grid_size);

			min_guess_count = grid_size + 1;
			min_guess_cell_index = -1;
			min_cell_guesses = 0;

			for (int i = 0; i < grid_length; i++)
			{
				//int* cell_guesses = /* (int*) */calloc(grid_size, sizeof(int));
				int* cell_guesses = malloc(grid_data_size);
				if (!cell_guesses)
				{
					printf("Memory allocation error in function 'solve'.");
					return 0;
				}

				int guess_count = bit_numbers(_c[i], grid_size, cell_guesses);
				if (guess_count != 0 && guess_count < min_guess_count)
				{
					min_guess_count = guess_count;
					min_guess_cell_index = i;
					if (min_cell_guesses != 0)
						free(min_cell_guesses);
					min_cell_guesses = cell_guesses;
				}
				else
				{
					free(cell_guesses);
				}
			}

			if (min_guess_cell_index >= 0)
			{
				stack_index++;
				grid_stack[stack_index] = _g;
				guess_stack[stack_index] = _c;
				min_guess_cell_index_stack[stack_index] = min_guess_cell_index;
				min_cell_guesses_stack[stack_index] = min_cell_guesses;
				cur_index_stack[stack_index] = 0;
				leaf_count_stack[stack_index] = min_guess_count;

				cur_index = 0;
				grid_leaf = _g;
				guess_leaf = _c;
			}
			else
			{
				free(_g);
				free(_c);
				free(min_cell_guesses);
			}
		}
	}
}

void test(int turn_count)
{
	int* result;
	int index = 0;
	int grid_size = 0;

	for (int i = 0; i < turn_count; i++)
	{
		//int* grid = read_hex_grid("input.txt", &grid_size);
		int* grid = read_grid("input.txt", &grid_size);

		if (!grid)
		{
			printf("File read error.");

			char s[10];
			scanf("%s", &s);
			return;
		}

		//for (int i = 0; i < grid_size; i++)
		//{
		//	for (int j = 0; j < grid_size; j++)
		//	{
		//		printf("%d,", grid[index]);
		//		index++;
		//	}
		//	puts("");
		//}

		int* guesses = /* (int*) */calloc(grid_size * grid_size, sizeof(int));

		initialize(guesses, grid, grid_size);

		puts("");

		//index = 0;

		//for (int i = 0; i < grid_size; i++)
		//{
		//	for (int j = 0; j < grid_size; j++)
		//	{
		//		printf("%d,", guesses[index]);
		//		index++;
		//	}
		//	puts("");
		//}


		time_t start = time(0);

		result = solve(grid, guesses, grid_size);

		time_t end = time(0);

		printf("%f\n", difftime(end, start));
	}

	index = 0;

	puts("");

	for (int i = 0; i < grid_size; i++)
	{
		for (int j = 0; j < grid_size; j++)
		{
			printf("%d,", result[index]);
			index++;
		}
		puts("");
	}

	free(result);

	char s[10];
	scanf("%s", &s);
}