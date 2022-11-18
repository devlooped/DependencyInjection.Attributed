using System.Collections.Generic;

namespace Library;

public partial record DuckEvent(string Message);

public partial record Point(int X, int Y);

public partial record Line(Point Start, Point End);

public partial record Buffer(ICollection<Line> Lines);

public partial record OnDidEdit(Buffer Buffer);

public record OnDidDrawLine(Line Line);