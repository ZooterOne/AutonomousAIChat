using System.Text;
using OllamaSharp;

namespace AutonomousAIChat;

/// <summary>
/// Represents an AI persona with specific traits, interests, and conversational styles.
/// </summary>
internal sealed class AIPersona
{
    private OllamaApiClient m_ollamaClient;

    /// <summary>
    /// Defines the gender identity of the persona.
    /// </summary>
    public enum GenderIdentity
    {
        Undefined,
        Male,
        Female
    }

    /// <summary>
    /// Defines the current tone of the persona.
    /// </summary>
    public enum Tone
    {
        None,
        Amused,
        Appreciative,
        Assertive,
        Bold,
        Calm,
        Candid,
        Cheerful,
        Comforting,
        Comic,
        Compassionate,
        Curious,
        Direct,
        Eloquent,
        Enthusiastic,
        Optimistic,
        Reassuring,
        Respectful,
        Romantic,
        Straightforward,
        Thoughtful,
        Tolerant
    }

    /// <summary>
    /// Defines the topics that the persona is interested in.
    /// </summary>
    [Flags]
    public enum Interest
    {
        Undefined = 0,
        Politics = 1,
        Movies = 2,
        Music = 4,
        Sport = 8,
        Technology = 16,
        Travel = 32,
        Science = 64,
        Animals = 128,
        Camping = 256,
        Gardening = 512,
        Writing = 1024,
        Reading = 2048,
        Cooking = 4096
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="AIPersona"/> class.
    /// </summary>
    /// <param name="name">The name of the persona.</param>
    /// <param name="ollamaClient">The Ollama API client used for communication.</param>
    public AIPersona(string name, OllamaApiClient ollamaClient)
    {
        Name = name;
        m_ollamaClient = ollamaClient;
    }

    /// <summary>
    /// Gets the name of the persona.
    /// </summary>
    public string Name { get; init; }

    /// <summary>
    /// Gets or sets the gender type of the persona.
    /// </summary>
    public GenderIdentity Gender
    {
        get;
        set
        {
            field = value;
            Chat = null;
        }
    } = GenderIdentity.Undefined;

    /// <summary>
    /// Gets or sets whether the conversation should be continuous.
    /// </summary>
    public bool EndlessConversation
    {
        get;
        set
        {
            field = value;
            Chat = null;
        }
    } = false;

    /// <summary>
    /// Gets or sets whether the persona should provide brief responses.
    /// </summary>
    public bool ShortAnswers
    {
        get;
        set
        {
            field = value;
            Chat = null;
        }
    } = true;

    /// <summary>
    /// Gets or sets the current emotional spirit of the persona.
    /// </summary>
    public Tone Spirit
    {
        get;
        set
        {
            field = value;
            Chat = null;
        }
    } = Tone.None;

    /// <summary>
    /// Gets or sets the set of interests for the persona.
    /// </summary>
    public Interest Interests
    {
        get;
        set
        {
            field = value;
            Chat = null;
        }
    } = Interest.Undefined;

    /// <summary>
    /// Gets or sets additional details for the persona.
    /// </summary>
    public string AdditionalDetails
    {
        get;
        set
        {
            field = value;
            Chat = null;
        }
    } = string.Empty;

    /// <summary>
    /// Gets the active conversation session for this persona. 
    /// </summary>
    public Chat? Chat
    {
        get
        {
            field ??= new Chat(m_ollamaClient, GeneratePrompt());
            return field;
        }
        private set;
    }

    private static string PersonaTypeAsPromptString(GenderIdentity type)
    {
        return type switch
        {
            GenderIdentity.Undefined => "person",
            GenderIdentity.Male => "male",
            GenderIdentity.Female => "female",
            _ => throw new ArgumentOutOfRangeException(nameof(type), type, null)
        };
    }

    private static void AppendSpiritToPrompt(Tone spirit, StringBuilder prompt)
    {
        switch (spirit)
        {
            case Tone.None:
                break;

            default:
                prompt.Append($" You are {spirit.ToString()}.");
                break;
        }
    }

    private static void AppendInterestsToPrompt(Interest interests, StringBuilder prompt)
    {
        var enums = Enum.GetValues(typeof(Interest)).Cast<Interest>()
            .Where(interest => interest != Interest.Undefined && ((interests & interest) == interest)).ToList();
        if (!enums.Any())
        {
            return;
        }
        var interestsAsString = string.Join(" and ", enums.Select(item => item.ToString()));
        prompt.Append($" You like to talk about {interestsAsString}.");
    }

    private string GeneratePrompt()
    {
        var prompt = new StringBuilder();
        prompt.Append($"Your are a {PersonaTypeAsPromptString(Gender)} engaging in a conversation.");
        if (EndlessConversation)
        {
            prompt.Append(" The conversation must never come to an end.");
        }

        if (ShortAnswers)
        {
            prompt.Append(" Keep your answers short.");
        }

        AppendSpiritToPrompt(Spirit, prompt);
        AppendInterestsToPrompt(Interests, prompt);
        if (!string.IsNullOrEmpty(AdditionalDetails))
        {
            prompt.Append($" {AdditionalDetails}");
        }

        return prompt.ToString();
    }
}
