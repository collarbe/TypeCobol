﻿using System;

namespace TypeCobol.Compiler.CodeElements
{
    public class UnstringStatementEnd : CodeElementEnd
    {
        public UnstringStatementEnd() : base(CodeElementType.UnstringStatementEnd)
        { }

        public override R Accept<R, D>(ICodeElementVisitor<R, D> v, D data)
        {
            return v.Visit(this, data);
        }
    }
}
