﻿using System;

namespace TypeCobol.Compiler.CodeElements
{
    public class MultiplyStatementEnd : CodeElementEnd
    {
        public MultiplyStatementEnd() : base(CodeElementType.MultiplyStatementEnd)
        { }

        public override R Accept<R, D>(ICodeElementVisitor<R, D> v, D data)
        {
            return v.Visit(this, data);
        }
    }
}
