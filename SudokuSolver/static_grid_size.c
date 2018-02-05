#include "static_grid_size.h"

void initialize_sgs(int* guesses, int* grid)
{
	int all_yes = pow(2, GRID_SIZE) - 1;
	//printf("all yes - %d\n", all_yes);
	for (int i = 0; i < GRID_LENGTH; i++)
	{
		guesses[i] = all_yes;
	}

	for (int i = 0; i < GRID_LENGTH; i++)
	{
		add_value_sgs(i, grid[i] - 1, guesses);
	}
}

void read_grid_sgs(char* fname, int* grid)
{
	int grid_size = 0;
	FILE* f = fopen(fname, "r");
	if (!f)
	{
		printf("Cann't open file %s.", fname);
		return;
	}

	int row[GRID_SIZE];
	int row_num = 0;

	while (!feof(f))
	{
		// how define the buffer size?
		char str[100];
		if (!fgets(str, 100, f))
		{
			printf("File read error. %s", fname);
			return;
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
			if (row_size != GRID_SIZE)
			{
				printf("A file for grid size %d required. This file contains a grid with size %d", GRID_SIZE, row_size);
				return;
			}

			grid_size = row_size;
		}
		else
		{
			if (grid_size != row_size)
			{
				printf("Grid isn't rectangle. %s", fname);
				return;
			}
		}

		for (int i = 0; i < GRID_SIZE; i++)
		{
			grid[row_num * GRID_SIZE + i] = row[i];
		}

		if (row_size > 0)
			row_num++;
	}

	fclose(f);
}

void read_hex_grid_sgs(char* fname, int* grid)
{
	int grid_size = 0;
	FILE* f = fopen(fname, "r");
	if (!f)
	{
		printf("Cann't open file %s.", fname);
		return;
	}

	int row[GRID_SIZE];
	int row_num = 0;
	while (!feof(f))
	{
		char str[100];
		if (!fgets(str, 100, f))
		{
			printf("File read error. %s", fname);
			return;
		}

		if (strlen(str) < 2)
			continue;

		int row_size = 0;
		int is_end_line_found = 0;
		for (int i = 0; i < 100; i++)
		{
			int c = str[i];

			if (row_size == GRID_SIZE)
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

		if (!is_end_line_found)
		{
			printf("File read error. %s\n", fname);
			return;
		}

		if (grid_size == 0)
		{
			if (row_size != GRID_SIZE)
			{
				printf("A file for grid size %d required. This file contains a grid with size %d\n", GRID_SIZE, row_size);
				return;
			}

			grid_size = row_size;
		}
		else
		{
			if (grid_size != row_size)
			{
				printf("Grid isn't rectangle. %s\n", fname);
				return;
			}
		}

		memcpy(grid + row_num * grid_size, row, grid_size * sizeof(int));

		if (row_size > 0)
			row_num++;
	}

	fclose(f);

	if (row_num != grid_size)
	{
		printf("Grid isn't rectangle. %s\n", fname);
		return;
	}
}

void add_value_sgs(int index, int value, int* guesses)
{
	if (value < 0)
		return;

	int x = index % GRID_SIZE;
	int y = index / GRID_SIZE;
	int mask = ~(1 << value);

	for (int i = 0; i < GRID_SIZE; i++)
	{
		guesses[i * GRID_SIZE + x] &= mask;
		guesses[y * GRID_SIZE + i] &= mask;
	}

	for (int i = (x / CELL_SIZE) * CELL_SIZE; i < (x / CELL_SIZE) * CELL_SIZE + CELL_SIZE; i++)
	{
		for (int j = (y / CELL_SIZE) * CELL_SIZE; j < (y / CELL_SIZE) * CELL_SIZE + CELL_SIZE; j++)
		{
			if (i != x && j != y)
			{
				guesses[j * GRID_SIZE + i] &= mask;
			}
		}
	}

	guesses[index] = 0;
}

int bit_numbers_sgs(int n, int* bits)
{
	if (n == 0)
		return 0;

	int index = 0;

	for (int i = 0; i < GRID_SIZE; i++)
	{
		if ((n & (1 << i)) != 0)
		{
			*(bits + index) = i;
			index++;
		}
	}

	return index;
}

int is_zero_exists_sgs(int* grid)
{
	for (int i = 0; i < GRID_LENGTH; i++)
	{
		if (grid[i] == 0)
			return 1;
	}
	return 0;
}

int solve_sgs(int* grid, int* guesses, int* result)
{
	int grid_data_length = GRID_LENGTH * sizeof(int);
	int grid_data_size = GRID_SIZE * sizeof(int);

	int grid_stack[STACK_SIZE];
	int guess_stack[STACK_SIZE];
	int min_guess_cell_index_stack[GRID_LENGTH];
	int min_cell_guesses_stack[SMALL_STACK_SIZE];
	int cur_index_stack[GRID_LENGTH];
	int leaf_count_stack[GRID_LENGTH];

	int stack_index = 0;

	int min_guess_count = GRID_SIZE + 1;
	int min_guess_cell_index = -1;

	int cell_guesses[GRID_SIZE];
	int zerro_count = 0;
	for (int i = 0; i < GRID_LENGTH; i++)
	{
		if (grid[i] == 0)
			zerro_count++;

		int guess_count = bit_numbers_sgs(guesses[i], cell_guesses);
		if (guess_count != 0 && guess_count < min_guess_count)
		{
			min_guess_count = guess_count;
			min_guess_cell_index = i;
			memcpy(min_cell_guesses_stack, cell_guesses, grid_data_size);
		}
	}

	if (min_guess_cell_index < 0)
	{
		memcpy(result, grid, grid_data_length);
		return 1;
	}

	memcpy(grid_stack, grid, grid_data_length);
	memcpy(guess_stack, guesses, grid_data_length);
	min_guess_cell_index_stack[stack_index] = min_guess_cell_index;
	leaf_count_stack[stack_index] = min_guess_count;
	cur_index_stack[stack_index] = -1;

	int* grid_leaf = grid_stack;
	int* guess_leaf = guess_stack;
	int* min_cell_guesses = min_cell_guesses_stack;

	while (stack_index >= 0)
	{
		grid_leaf = grid_stack + stack_index * GRID_LENGTH;
		guess_leaf = guess_stack + stack_index * GRID_LENGTH;
		min_cell_guesses = min_cell_guesses_stack + stack_index * GRID_SIZE;

		min_guess_cell_index = min_guess_cell_index_stack[stack_index];
		min_guess_count = leaf_count_stack[stack_index];
		int cur_index = cur_index_stack[stack_index];
		cur_index++;

		if (cur_index == min_guess_count)
		{
			stack_index--;
			continue;
		}
		else
		{
			cur_index_stack[stack_index] = cur_index;
		}

		while (min_guess_cell_index >= 0)
		{
			//if (stack_index < 2)
			//{
			//	printf("\n%d\n", min_guess_cell_index);
			//	for (int i = 0; i < min_guess_count; i++)
			//		printf("%d,", min_cell_guesses_stack[stack_index * GRID_SIZE + i]);
			//}

			int* next_grid_leaf = grid_leaf + GRID_LENGTH;
			memcpy(next_grid_leaf, grid_leaf, grid_data_length);
			next_grid_leaf[min_guess_cell_index] = min_cell_guesses[cur_index] + 1;

			if (stack_index == zerro_count - 1)
			{
				if (!is_zero_exists_sgs(next_grid_leaf))
				{
					memcpy(result, next_grid_leaf, grid_data_length);
					return 0;
				}
			}

			int* next_guess_leaf = guess_leaf + GRID_LENGTH;
			memcpy(next_guess_leaf, guess_leaf, grid_data_length);
			add_value_sgs(min_guess_cell_index, min_cell_guesses[cur_index], next_guess_leaf);

			min_guess_count = GRID_SIZE + 1;
			min_guess_cell_index = -1;

			for (int i = 0; i < GRID_LENGTH; i++)
			{
				int guess_count = bit_numbers_sgs(next_guess_leaf[i], cell_guesses);
				if (guess_count != 0 && guess_count < min_guess_count)
				{
					min_guess_count = guess_count;
					min_guess_cell_index = i;
					memcpy(min_cell_guesses + GRID_SIZE, cell_guesses, grid_data_size);
				}
			}

			if (min_guess_cell_index >= 0)
			{
				stack_index++;

				cur_index = 0;
				cur_index_stack[stack_index] = cur_index;

				grid_leaf += GRID_LENGTH;
				guess_leaf += GRID_LENGTH;
				min_cell_guesses += GRID_SIZE;

				min_guess_cell_index_stack[stack_index] = min_guess_cell_index;
				leaf_count_stack[stack_index] = min_guess_count;
			}
		}
	}

	return 1;
}

void test_sgs(int turn_count)
{
	int result[GRID_LENGTH];
	int index = 0;

	int exit_code = 0;

	for (int i = 0; i < turn_count; i++)
	{
		
		int grid[GRID_LENGTH];
		read_grid_sgs("input.txt", grid);
		//read_hex_grid_sgs("input.txt", grid);

		if (!grid)
		{
			printf("File read error.");

			char s[10];
			scanf("%s", &s);
			return;
		}

		//index = 0;
		//for (int i = 0; i < GRID_SIZE; i++)
		//{
		//	for (int j = 0; j < GRID_SIZE; j++)
		//	{
		//		printf("%d,", grid[index]);
		//		index++;
		//	}
		//	puts("");
		//}

		int* guesses = /* (int*) */calloc(GRID_LENGTH, sizeof(int));

		initialize_sgs(guesses, grid);

		//puts("");

		//index = 0;
		//for (int i = 0; i < GRID_SIZE; i++)
		//{
		//	for (int j = 0; j < GRID_SIZE; j++)
		//	{
		//		printf("%d,", guesses[index]);
		//		index++;
		//	}
		//	puts("");
		//}


		time_t start = time(0);

		exit_code = solve_sgs_h(grid, guesses, result);
		//exit_code = solve_sgs(grid, guesses, result);

		time_t end = time(0);

		printf("%f\n", difftime(end, start));

	}

	if (exit_code > 0)
	{
		printf("Position hasn't solution.\n");
	}
	else
	{
		puts("");
		print_grid("output.txt", result);
	}

	char s[10];
	scanf("%s", &s);
}

void print_grid(char* fname, int* grid)
{
	FILE* f = fopen(fname, "a+");
	if (!f)
	{
		printf("Cann't open file %s\n", fname);
		return;
	}

	int index = 0;
	for (int i = 0; i < GRID_SIZE; i++)
	{
		for (int j = 0; j < GRID_SIZE; j++)
		{
			fprintf(f, "%d,", grid[index]);
			printf("%d,", grid[index]);
			index++;
		}

		fputs("\n", f);
		puts("");
	}

	fputs("\n", f);
	fclose(f);
}

int solve_sgs_h(int* grid, int* guesses, int* result)
{
	int grid_data_length = GRID_LENGTH * sizeof(int);
	int grid_data_size = GRID_SIZE * sizeof(int);
	int stack_data_size = STACK_SIZE * sizeof(int);

	int* grid_stack = (int*)malloc(stack_data_size);
	int* guess_stack = (int*)malloc(stack_data_size);
	int* min_guess_cell_index_stack = (int*)malloc(grid_data_length);
	int* min_cell_guesses_stack = (int*)malloc(SMALL_STACK_SIZE * sizeof(int));
	int* cur_index_stack = (int*)malloc(grid_data_length);
	int* leaf_count_stack = (int*)malloc(grid_data_length);

	int stack_index = 0;

	int min_guess_count = GRID_SIZE + 1;
	int min_guess_cell_index = -1;

	int cell_guesses[GRID_SIZE];
	int zerro_count = 0;
	for (int i = 0; i < GRID_LENGTH; i++)
	{
		if (grid[i] == 0)
			zerro_count++;

		int guess_count = bit_numbers_sgs(guesses[i], cell_guesses);
		if (guess_count != 0 && guess_count < min_guess_count)
		{
			min_guess_count = guess_count;
			min_guess_cell_index = i;
			memcpy(min_cell_guesses_stack, cell_guesses, grid_data_size);
		}
	}

	if (min_guess_cell_index < 0)
	{
		memcpy(result, grid, grid_data_length);
		return 1;
	}

	memcpy(grid_stack, grid, grid_data_length);
	memcpy(guess_stack, guesses, grid_data_length);
	min_guess_cell_index_stack[stack_index] = min_guess_cell_index;
	leaf_count_stack[stack_index] = min_guess_count;
	cur_index_stack[stack_index] = -1;

	int* grid_leaf = grid_stack;
	int* guess_leaf = guess_stack;
	int* min_cell_guesses = min_cell_guesses_stack;

	while (stack_index >= 0)
	{
		grid_leaf = grid_stack + stack_index * GRID_LENGTH;
		guess_leaf = guess_stack + stack_index * GRID_LENGTH;
		min_cell_guesses = min_cell_guesses_stack + stack_index * GRID_SIZE;

		min_guess_cell_index = min_guess_cell_index_stack[stack_index];
		min_guess_count = leaf_count_stack[stack_index];
		int cur_index = cur_index_stack[stack_index];
		cur_index++;

		if (cur_index == min_guess_count)
		{
			stack_index--;
			continue;
		}
		else
		{
			cur_index_stack[stack_index] = cur_index;
		}

		while (min_guess_cell_index >= 0)
		{
			//if (stack_index < 2)
			//{
			//	printf("\n%d\n", min_guess_cell_index);
			//	for (int i = 0; i < min_guess_count; i++)
			//		printf("%d,", min_cell_guesses_stack[stack_index * GRID_SIZE + i]);
			//}

			int* next_grid_leaf = grid_leaf + GRID_LENGTH;
			memcpy(next_grid_leaf, grid_leaf, grid_data_length);
			next_grid_leaf[min_guess_cell_index] = min_cell_guesses[cur_index] + 1;

			if (stack_index == zerro_count - 1)
			{
				if (!is_zero_exists_sgs(next_grid_leaf))
				{
					memcpy(result, next_grid_leaf, grid_data_length);
					return 0;
				}
			}

			int* next_guess_leaf = guess_leaf + GRID_LENGTH;
			memcpy(next_guess_leaf, guess_leaf, grid_data_length);
			add_value_sgs(min_guess_cell_index, min_cell_guesses[cur_index], next_guess_leaf);

			min_guess_count = GRID_SIZE + 1;
			min_guess_cell_index = -1;

			for (int i = 0; i < GRID_LENGTH; i++)
			{
				int guess_count = bit_numbers_sgs(next_guess_leaf[i], cell_guesses);
				if (guess_count != 0 && guess_count < min_guess_count)
				{
					min_guess_count = guess_count;
					min_guess_cell_index = i;
					memcpy(min_cell_guesses + GRID_SIZE, cell_guesses, grid_data_size);
				}
			}

			if (min_guess_cell_index >= 0)
			{
				stack_index++;

				cur_index = 0;
				cur_index_stack[stack_index] = cur_index;

				grid_leaf += GRID_LENGTH;
				guess_leaf += GRID_LENGTH;
				min_cell_guesses += GRID_SIZE;

				min_guess_cell_index_stack[stack_index] = min_guess_cell_index;
				leaf_count_stack[stack_index] = min_guess_count;
			}
		}
	}

	return 1;
}