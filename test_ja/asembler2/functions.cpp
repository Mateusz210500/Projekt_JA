#include <Windows.h>
#define function _declspec(dllexport)
extern "C" {
	int _stdcall MyProc1(DWORD x, DWORD y, DWORD z);
	function int adding(int a, int b, int c)
	{
		return MyProc1(a, b, c);
	}
}
