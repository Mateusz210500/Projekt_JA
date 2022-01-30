.DATA 
dividend  DWORD ?
divisor  DWORD ?
.CODE 

Blur proc x: DWORD, y: DWORD, z: DWORD 
mov eax, ECX 
imul eax, eax
mov dividend, eax
mov eax, 0
mov eax, EDX 
imul eax, eax
mov ecx, 10000
mul ecx
add dividend, eax 
mov eax, 0

mov eax, ebx
imul eax, eax
mov ecx, 2
mul ecx
mov divisor, eax

mov eax, dividend
mov ecx, divisor
div ecx

ret
Blur endp 
END 