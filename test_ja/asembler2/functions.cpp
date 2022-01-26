#include <Windows.h>
#define function _declspec(dllexport)
extern "C" {
	int _stdcall MyProc1(DWORD x, DWORD y);
	function int adding(int a, double b)
	{
		return MyProc1(a, b);
	}
}
