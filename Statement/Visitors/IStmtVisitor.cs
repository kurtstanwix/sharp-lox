﻿namespace SharpLox.Statement.Visitors;

public interface IStmtVisitor<TReturn>
{
    TReturn VisitBlockStmt(Block stmt);
    TReturn VisitClassStmt(Class stmt);
    TReturn VisitExpressionStmt(Expression stmt);
    TReturn VisitFunctionStmt(Function stmt);
    TReturn VisitIfStmt(If stmt);
    TReturn VisitPrintStmt(Print stmt);
    TReturn VisitReturnStmt(Return stmt);
    TReturn VisitVarStmt(Var stmt);
    TReturn VisitWhileStmt(While stmt);
}
