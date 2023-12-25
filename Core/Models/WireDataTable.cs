namespace Core.Models;

/// <summary>
/// Data table representation suitable for network (wire) transmission
/// </summary>
/// <remarks>
/// This generic representation makes it easy to display data without worrying over 
/// data types of each column
/// </remarks>
public class WireDataTable
{
    public Column[] Columns { get; set; } = [];

    public Row[] Rows { get; set; } = [];

    public WireDataTable() { }
};

public class Column
{
    public string Title { get; set; } = string.Empty;

    public bool IsRight { get; set; }
}

public class Row
{
    public string[] Cells { get; set; } = [];

    public Row() { }

    public Row(params string[] cells) 
    { 
        Cells = cells;
    }
}
