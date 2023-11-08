using SharpLox.Statement.Visitors;

namespace SharpLox.Statement;

public interface IStmt
{
    TReturn Accept<TReturn>(IStmtVisitor<TReturn> visitor);
}
