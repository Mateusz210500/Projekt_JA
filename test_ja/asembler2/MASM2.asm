.DATA 
dividend  DWORD ?
divisor  DWORD ?
.CODE 

Blur proc
imul ecx, ecx
mov dividend, ecx
mov eax, 0
imul edx, edx
add dividend, edx 
mov eax, dividend
imul eax, 100
mov dividend, eax
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