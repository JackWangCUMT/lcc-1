﻿#include <stdio.h>
#include <stdlib.h>

#define assert(cond, msg) if (!(cond)) {    \
    printf("assert fail: %s\n", msg);       \
    exit(-1);                               \
}

struct ListNode {
    int val;
    struct ListNode *next;
};

int hasCycle(struct ListNode* head);

int main(int argc, char* argv[]) {
    struct ListNode x1, x2, x3, x4, x5, x6, x7;
    x1.val = 1;
    x2.val = 2;
    x3.val = 3;
    x4.val = 4;
    x5.val = 5;
    x1.next = &x2;
    x2.next = &x3;
    x3.next = &x4;
    x4.next = &x5;
    x5.next = 0;
    x6.val = 6;
    x7.val = 7;
    x6.next = &x7;
    x7.next = 0;
    assert(hasCycle(&x1) == 0, "does not have cycle");
    printf("everything is fine!\n");
    return 0;
}