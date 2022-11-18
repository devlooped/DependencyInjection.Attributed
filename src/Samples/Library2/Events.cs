using System.Collections.Generic;

namespace Library;

public partial record DuckEvent(string Message);

public partial record Point(int X, int Y);

public partial record Line(Point Start, Point End);

public partial record Buffer(Line[] Lines);

public partial record OnDidEdit(Buffer Buffer);

public partial record OnDidDrawLine(Line Line)
{
    //public static OnDidDrawLine Create(dynamic value) => new(Line.Create(value.Line));
}