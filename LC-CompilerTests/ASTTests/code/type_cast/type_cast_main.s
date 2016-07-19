	.text
	.def	 @feat.00;
	.scl	3;
	.type	0;
	.endef
	.globl	@feat.00
@feat.00 = 1
	.intel_syntax noprefix
	.def	 _sprintf;
	.scl	2;
	.type	32;
	.endef
	.section	.text,"xr",discard,_sprintf
	.globl	_sprintf
	.p2align	4, 0x90
_sprintf:                               # @sprintf
# BB#0:
	push	ebp
	mov	ebp, esp
	push	esi
	sub	esp, 32
	mov	eax, dword ptr [ebp + 12]
	mov	ecx, dword ptr [ebp + 8]
	mov	dword ptr [ebp - 8], eax
	mov	dword ptr [ebp - 12], ecx
	lea	eax, [ebp + 16]
	mov	dword ptr [ebp - 20], eax
	mov	ecx, dword ptr [ebp - 8]
	mov	edx, dword ptr [ebp - 12]
	mov	esi, esp
	mov	dword ptr [esi + 12], eax
	mov	dword ptr [esi + 4], ecx
	mov	dword ptr [esi], edx
	mov	dword ptr [esi + 8], 0
	call	__vsprintf_l
	mov	dword ptr [ebp - 16], eax
	add	esp, 32
	pop	esi
	pop	ebp
	ret

	.def	 _vsprintf;
	.scl	2;
	.type	32;
	.endef
	.section	.text,"xr",discard,_vsprintf
	.globl	_vsprintf
	.p2align	4, 0x90
_vsprintf:                              # @vsprintf
# BB#0:
	push	ebp
	mov	ebp, esp
	push	edi
	push	esi
	sub	esp, 40
	mov	eax, dword ptr [ebp + 16]
	mov	ecx, dword ptr [ebp + 12]
	mov	edx, dword ptr [ebp + 8]
	mov	esi, 4294967295
	xor	edi, edi
	mov	dword ptr [ebp - 12], eax
	mov	dword ptr [ebp - 16], ecx
	mov	dword ptr [ebp - 20], edx
	mov	eax, dword ptr [ebp - 12]
	mov	ecx, dword ptr [ebp - 16]
	mov	edx, dword ptr [ebp - 20]
	mov	dword ptr [esp], edx
	mov	dword ptr [esp + 4], -1
	mov	dword ptr [esp + 8], ecx
	mov	dword ptr [esp + 12], 0
	mov	dword ptr [esp + 16], eax
	mov	dword ptr [ebp - 24], edi # 4-byte Spill
	mov	dword ptr [ebp - 28], esi # 4-byte Spill
	call	__vsnprintf_l
	add	esp, 40
	pop	esi
	pop	edi
	pop	ebp
	ret

	.def	 __snprintf;
	.scl	2;
	.type	32;
	.endef
	.section	.text,"xr",discard,__snprintf
	.globl	__snprintf
	.p2align	4, 0x90
__snprintf:                             # @_snprintf
# BB#0:
	push	ebp
	mov	ebp, esp
	push	edi
	push	esi
	sub	esp, 36
	mov	eax, dword ptr [ebp + 16]
	mov	ecx, dword ptr [ebp + 12]
	mov	edx, dword ptr [ebp + 8]
	mov	dword ptr [ebp - 12], eax
	mov	dword ptr [ebp - 16], ecx
	mov	dword ptr [ebp - 20], edx
	lea	eax, [ebp + 20]
	mov	dword ptr [ebp - 28], eax
	mov	ecx, dword ptr [ebp - 12]
	mov	edx, dword ptr [ebp - 16]
	mov	esi, dword ptr [ebp - 20]
	mov	edi, esp
	mov	dword ptr [edi + 12], eax
	mov	dword ptr [edi + 8], ecx
	mov	dword ptr [edi + 4], edx
	mov	dword ptr [edi], esi
	call	__vsnprintf
	mov	dword ptr [ebp - 24], eax
	add	esp, 36
	pop	esi
	pop	edi
	pop	ebp
	ret

	.def	 __vsnprintf;
	.scl	2;
	.type	32;
	.endef
	.section	.text,"xr",discard,__vsnprintf
	.globl	__vsnprintf
	.p2align	4, 0x90
__vsnprintf:                            # @_vsnprintf
# BB#0:
	push	ebp
	mov	ebp, esp
	push	edi
	push	esi
	sub	esp, 40
	mov	eax, dword ptr [ebp + 20]
	mov	ecx, dword ptr [ebp + 16]
	mov	edx, dword ptr [ebp + 12]
	mov	esi, dword ptr [ebp + 8]
	xor	edi, edi
	mov	dword ptr [ebp - 12], eax
	mov	dword ptr [ebp - 16], ecx
	mov	dword ptr [ebp - 20], edx
	mov	dword ptr [ebp - 24], esi
	mov	eax, dword ptr [ebp - 12]
	mov	ecx, dword ptr [ebp - 16]
	mov	edx, dword ptr [ebp - 20]
	mov	esi, dword ptr [ebp - 24]
	mov	dword ptr [esp], esi
	mov	dword ptr [esp + 4], edx
	mov	dword ptr [esp + 8], ecx
	mov	dword ptr [esp + 12], 0
	mov	dword ptr [esp + 16], eax
	mov	dword ptr [ebp - 28], edi # 4-byte Spill
	call	__vsnprintf_l
	add	esp, 40
	pop	esi
	pop	edi
	pop	ebp
	ret

	.def	 _main;
	.scl	2;
	.type	32;
	.endef
	.text
	.globl	_main
	.p2align	4, 0x90
_main:                                  # @main
# BB#0:
	push	ebp
	mov	ebp, esp
	push	esi
	sub	esp, 32
	mov	eax, dword ptr [ebp + 12]
	mov	ecx, dword ptr [ebp + 8]
	mov	dword ptr [ebp - 8], 0
	mov	dword ptr [ebp - 12], eax
	mov	dword ptr [ebp - 16], ecx
	mov	byte ptr [ebp - 17], -128
	movzx	eax, byte ptr [ebp - 17]
	mov	dword ptr [esp], eax
	call	_uchar2schar
	lea	ecx, ["??_C@_0BG@CHCICMIM@uchar2schar?$CI?$CFd?$CJ?5?$DN?5?$CFd?6?$AA@"]
	movsx	edx, al
	movzx	esi, byte ptr [ebp - 17]
	mov	dword ptr [esp], ecx
	mov	dword ptr [esp + 4], esi
	mov	dword ptr [esp + 8], edx
	call	_printf
	xor	ecx, ecx
	mov	dword ptr [ebp - 24], eax # 4-byte Spill
	mov	eax, ecx
	add	esp, 32
	pop	esi
	pop	ebp
	ret

	.def	 _printf;
	.scl	2;
	.type	32;
	.endef
	.section	.text,"xr",discard,_printf
	.globl	_printf
	.p2align	4, 0x90
_printf:                                # @printf
# BB#0:
	push	ebp
	mov	ebp, esp
	push	esi
	sub	esp, 36
	mov	eax, dword ptr [ebp + 8]
	mov	dword ptr [ebp - 8], eax
	lea	eax, [ebp + 12]
	mov	dword ptr [ebp - 16], eax
	mov	ecx, dword ptr [ebp - 8]
	mov	edx, esp
	mov	dword ptr [edx], 1
	mov	dword ptr [ebp - 20], eax # 4-byte Spill
	mov	dword ptr [ebp - 24], ecx # 4-byte Spill
	call	___acrt_iob_func
	mov	ecx, esp
	mov	edx, dword ptr [ebp - 20] # 4-byte Reload
	mov	dword ptr [ecx + 12], edx
	mov	esi, dword ptr [ebp - 24] # 4-byte Reload
	mov	dword ptr [ecx + 4], esi
	mov	dword ptr [ecx], eax
	mov	dword ptr [ecx + 8], 0
	call	__vfprintf_l
	mov	dword ptr [ebp - 12], eax
	add	esp, 36
	pop	esi
	pop	ebp
	ret

	.def	 __vsprintf_l;
	.scl	2;
	.type	32;
	.endef
	.section	.text,"xr",discard,__vsprintf_l
	.globl	__vsprintf_l
	.p2align	4, 0x90
__vsprintf_l:                           # @_vsprintf_l
# BB#0:
	push	ebp
	mov	ebp, esp
	push	edi
	push	esi
	sub	esp, 40
	mov	eax, dword ptr [ebp + 20]
	mov	ecx, dword ptr [ebp + 16]
	mov	edx, dword ptr [ebp + 12]
	mov	esi, dword ptr [ebp + 8]
	mov	edi, 4294967295
	mov	dword ptr [ebp - 12], eax
	mov	dword ptr [ebp - 16], ecx
	mov	dword ptr [ebp - 20], edx
	mov	dword ptr [ebp - 24], esi
	mov	eax, dword ptr [ebp - 12]
	mov	ecx, dword ptr [ebp - 16]
	mov	edx, dword ptr [ebp - 20]
	mov	esi, dword ptr [ebp - 24]
	mov	dword ptr [esp], esi
	mov	dword ptr [esp + 4], -1
	mov	dword ptr [esp + 8], edx
	mov	dword ptr [esp + 12], ecx
	mov	dword ptr [esp + 16], eax
	mov	dword ptr [ebp - 28], edi # 4-byte Spill
	call	__vsnprintf_l
	add	esp, 40
	pop	esi
	pop	edi
	pop	ebp
	ret

	.def	 __vsnprintf_l;
	.scl	2;
	.type	32;
	.endef
	.section	.text,"xr",discard,__vsnprintf_l
	.globl	__vsnprintf_l
	.p2align	4, 0x90
__vsnprintf_l:                          # @_vsnprintf_l
# BB#0:
	push	ebp
	mov	ebp, esp
	push	ebx
	push	edi
	push	esi
	sub	esp, 76
	mov	eax, dword ptr [ebp + 24]
	mov	ecx, dword ptr [ebp + 20]
	mov	edx, dword ptr [ebp + 16]
	mov	esi, dword ptr [ebp + 12]
	mov	edi, dword ptr [ebp + 8]
	mov	dword ptr [ebp - 16], eax
	mov	dword ptr [ebp - 20], ecx
	mov	dword ptr [ebp - 24], edx
	mov	dword ptr [ebp - 28], esi
	mov	dword ptr [ebp - 32], edi
	mov	eax, dword ptr [ebp - 16]
	mov	ecx, dword ptr [ebp - 20]
	mov	edx, dword ptr [ebp - 24]
	mov	esi, dword ptr [ebp - 28]
	mov	dword ptr [ebp - 40], esi # 4-byte Spill
	mov	dword ptr [ebp - 44], eax # 4-byte Spill
	mov	dword ptr [ebp - 48], ecx # 4-byte Spill
	mov	dword ptr [ebp - 52], edx # 4-byte Spill
	mov	dword ptr [ebp - 56], edi # 4-byte Spill
	call	___local_stdio_printf_options
	mov	ecx, dword ptr [eax]
	mov	eax, dword ptr [eax + 4]
	or	ecx, 1
	mov	edx, esp
	mov	esi, dword ptr [ebp - 44] # 4-byte Reload
	mov	dword ptr [edx + 24], esi
	mov	edi, dword ptr [ebp - 48] # 4-byte Reload
	mov	dword ptr [edx + 20], edi
	mov	ebx, dword ptr [ebp - 52] # 4-byte Reload
	mov	dword ptr [edx + 16], ebx
	mov	esi, dword ptr [ebp - 40] # 4-byte Reload
	mov	dword ptr [edx + 12], esi
	mov	esi, dword ptr [ebp - 56] # 4-byte Reload
	mov	dword ptr [edx + 8], esi
	mov	dword ptr [edx + 4], eax
	mov	dword ptr [edx], ecx
	call	___stdio_common_vsprintf
	mov	dword ptr [ebp - 36], eax
	cmp	dword ptr [ebp - 36], 0
	jge	LBB7_2
# BB#1:
	mov	eax, 4294967295
	mov	dword ptr [ebp - 60], eax # 4-byte Spill
	jmp	LBB7_3
LBB7_2:
	mov	eax, dword ptr [ebp - 36]
	mov	dword ptr [ebp - 60], eax # 4-byte Spill
LBB7_3:
	mov	eax, dword ptr [ebp - 60] # 4-byte Reload
	add	esp, 76
	pop	esi
	pop	edi
	pop	ebx
	pop	ebp
	ret

	.def	 ___local_stdio_printf_options;
	.scl	2;
	.type	32;
	.endef
	.section	.text,"xr",discard,___local_stdio_printf_options
	.globl	___local_stdio_printf_options
	.p2align	4, 0x90
___local_stdio_printf_options:          # @__local_stdio_printf_options
# BB#0:
	push	ebp
	mov	ebp, esp
	lea	eax, [___local_stdio_printf_options._OptionsStorage]
	pop	ebp
	ret

	.def	 __vfprintf_l;
	.scl	2;
	.type	32;
	.endef
	.section	.text,"xr",discard,__vfprintf_l
	.globl	__vfprintf_l
	.p2align	4, 0x90
__vfprintf_l:                           # @_vfprintf_l
# BB#0:
	push	ebp
	mov	ebp, esp
	push	ebx
	push	edi
	push	esi
	sub	esp, 56
	mov	eax, dword ptr [ebp + 20]
	mov	ecx, dword ptr [ebp + 16]
	mov	edx, dword ptr [ebp + 12]
	mov	esi, dword ptr [ebp + 8]
	mov	dword ptr [ebp - 16], eax
	mov	dword ptr [ebp - 20], ecx
	mov	dword ptr [ebp - 24], edx
	mov	dword ptr [ebp - 28], esi
	mov	eax, dword ptr [ebp - 16]
	mov	ecx, dword ptr [ebp - 20]
	mov	edx, dword ptr [ebp - 24]
	mov	dword ptr [ebp - 32], edx # 4-byte Spill
	mov	dword ptr [ebp - 36], eax # 4-byte Spill
	mov	dword ptr [ebp - 40], ecx # 4-byte Spill
	mov	dword ptr [ebp - 44], esi # 4-byte Spill
	call	___local_stdio_printf_options
	mov	ecx, dword ptr [eax]
	mov	eax, dword ptr [eax + 4]
	mov	edx, esp
	mov	esi, dword ptr [ebp - 36] # 4-byte Reload
	mov	dword ptr [edx + 20], esi
	mov	edi, dword ptr [ebp - 40] # 4-byte Reload
	mov	dword ptr [edx + 16], edi
	mov	ebx, dword ptr [ebp - 32] # 4-byte Reload
	mov	dword ptr [edx + 12], ebx
	mov	esi, dword ptr [ebp - 44] # 4-byte Reload
	mov	dword ptr [edx + 8], esi
	mov	dword ptr [edx + 4], eax
	mov	dword ptr [edx], ecx
	call	___stdio_common_vfprintf
	add	esp, 56
	pop	esi
	pop	edi
	pop	ebx
	pop	ebp
	ret

	.section	.rdata,"dr",discard,"??_C@_0BG@CHCICMIM@uchar2schar?$CI?$CFd?$CJ?5?$DN?5?$CFd?6?$AA@"
	.globl	"??_C@_0BG@CHCICMIM@uchar2schar?$CI?$CFd?$CJ?5?$DN?5?$CFd?6?$AA@" # @"\01??_C@_0BG@CHCICMIM@uchar2schar?$CI?$CFd?$CJ?5?$DN?5?$CFd?6?$AA@"
"??_C@_0BG@CHCICMIM@uchar2schar?$CI?$CFd?$CJ?5?$DN?5?$CFd?6?$AA@":
	.asciz	"uchar2schar(%d) = %d\n"

	.lcomm	___local_stdio_printf_options._OptionsStorage,8,8 # @__local_stdio_printf_options._OptionsStorage

