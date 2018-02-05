#ifndef DINAMIC_GRID_SIZE
#define DINAMIC_GRID_SIZE

#include "common.h"

int* read_hex_grid(char* file_name, int* grid_size);
void initialize(int* guesses, int* grid, int grid_size);
void add_value(int index, int value, int* guesses, int cell_size, int grid_size);
int bit_numbers(int n, int grid_size, int* bits);
int* solve(int* grid, int* guesses, int grid_size);
int is_zero_exists(int* grid, int grid_size);
int* read_grid(char* file_name, int* grid_size);
void test(int turn_count);

#endif
