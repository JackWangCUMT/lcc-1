﻿#include <stdio.h>

int func1(int argc, char* argv[]);
int test_if(int x);
int test_for(int x);
int sum(int x);

int main(int argc, char* argv[]) {
    for (int i = 0; i < 10; ++i) {
        printf("func1(%d) = %d\n", i, func1(i, 0));
    }
    for (int i = -4; i < 5; ++i) {
        printf("test_if(%d) = %d\n", i, test_if(i));
    }
    printf("test_for(%d) = %d\n", 10, test_for(10));
    printf("sum(%d) = %d\n", 10, sum(10));
    return 0;
}
