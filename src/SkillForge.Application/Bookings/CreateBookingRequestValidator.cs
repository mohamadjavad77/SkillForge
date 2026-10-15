using FluentValidation;

namespace SkillForge.Application.Bookings;

public sealed class CreateBookingRequestValidator : AbstractValidator<CreateBookingRequest>
{
    public CreateBookingRequestValidator()
    {
        RuleFor(x => x.SlotId).NotEmpty();

        RuleFor(x => x.MeetingLink)
            .MaximumLength(1000)
            .When(x => x.MeetingLink is not null);
    }
}
