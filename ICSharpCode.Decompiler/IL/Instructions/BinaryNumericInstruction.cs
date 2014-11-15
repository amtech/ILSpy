﻿// Copyright (c) 2014 Daniel Grunwald
// 
// Permission is hereby granted, free of charge, to any person obtaining a copy of this
// software and associated documentation files (the "Software"), to deal in the Software
// without restriction, including without limitation the rights to use, copy, modify, merge,
// publish, distribute, sublicense, and/or sell copies of the Software, and to permit persons
// to whom the Software is furnished to do so, subject to the following conditions:
// 
// The above copyright notice and this permission notice shall be included in all copies or
// substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED,
// INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR
// PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE
// FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR
// OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER
// DEALINGS IN THE SOFTWARE.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
namespace ICSharpCode.Decompiler.IL
{
	public abstract partial class BinaryNumericInstruction : BinaryInstruction
	{
		/// <summary>
		/// Gets whether the instruction checks for overflow.
		/// </summary>
		public readonly bool CheckForOverflow;
		
		/// <summary>
		/// For integer operations that depend on the sign, specifies whether the operation
		/// is signed or unsigned.
		/// For instructions that produce the same result for either sign, returns Sign.None.
		/// </summary>
		public readonly Sign Sign;

		readonly StackType resultType;

		protected BinaryNumericInstruction(OpCode opCode, ILInstruction left, ILInstruction right, bool checkForOverflow, Sign sign)
			: base(opCode, left, right)
		{
			this.CheckForOverflow = checkForOverflow;
			this.Sign = sign;
			this.resultType = ComputeResultType(opCode, left.ResultType, right.ResultType);
		}

		internal static StackType ComputeResultType(OpCode opCode, StackType left, StackType right)
		{
			// Based on Table 2: Binary Numeric Operations
			// also works for Table 5: Integer Operations
			// and for Table 7: Overflow Arithmetic Operations
			if (left == right) {
				return left;
			}
			if (left == StackType.Ref || right == StackType.Ref) {
				if (left == StackType.Ref && right == StackType.Ref) {
					// sub(&, &) = I
					Debug.Assert(opCode == OpCode.Sub);
					return StackType.I;
				} else {
					// add/sub with I or I4 and &
					Debug.Assert(opCode == OpCode.Add || opCode == OpCode.Sub);
					return StackType.Ref;
				}
			}
			if (left == StackType.I || right == StackType.I)
				return StackType.I;
			return StackType.Unknown;
		}
		
		public sealed override StackType ResultType {
			get {
				return resultType;
			}
		}

		protected override InstructionFlags ComputeFlags()
		{
			var flags = base.ComputeFlags();
			if (CheckForOverflow)
				flags |= InstructionFlags.MayThrow;
			return flags;
		}

		public override void WriteTo(ITextOutput output)
		{
			output.Write(OpCode);
			if (CheckForOverflow)
				output.Write(".ovf");
			if (Sign == Sign.Unsigned)
				output.Write(".unsigned");
			else if (Sign == Sign.Signed)
				output.Write(".signed");
			output.Write('(');
			Left.WriteTo(output);
			output.Write(", ");
			Right.WriteTo(output);
			output.Write(')');
		}
	}
}

