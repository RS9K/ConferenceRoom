namespace ConferenceRoom.Domain;

public class Booking
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string RoomId { get; set; } = default!;
    public DateTime Start { get; set; }
    public DateTime End { get; set; }
}
