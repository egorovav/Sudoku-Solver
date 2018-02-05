#ifndef STATIC_GRID_SIZE
#define STATIC_GRID_SIZE

#include "common.h"

#define GRID_SIZE 16
#define CELL_SIZE 4
#define GRID_LENGTH 256
#define STACK_SIZE 65536
#define SMALL_STACK_SIZE 4096

void read_hex_grid_sgs(char* file_name, int* grid);
void initialize_sgs(int* guesses, int* grid);
void add_value_sgs(int index, int value, int* guesses);
int bit_numbers_sgs(int n, int* bits);
int solve_sgs(int* grid, int* guesses, int* result);
int is_zero_exists_sgs(int* grid);
void read_grid_sgs(char* file_name, int* grid);
void test_sgs(int turn_count);
void print_grid(char* fname, int* grid);
int solve_sgs_h(int* grid, int* guesses, int* result);

#endif
