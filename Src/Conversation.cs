using System.Text;
using OllamaSharp.Models.Chat;

namespace AutonomousAIChat;

/// <summary>
/// Manages a conversation between multiple AI personas.
/// </summary>
internal sealed class Conversation
{
    private AIPersona[] m_aiPersona;
    private int m_currentPersonaIndex;
    private StringBuilder m_lastOutput;

    /// <summary>
    /// Gets the name of the persona currently speaking.
    /// </summary>
    public string CurrentPersonaName => m_aiPersona[m_currentPersonaIndex].Name;
    /// <summary>
    /// Initializes the conversation with two AI personas and a starting text.
    /// </summary>
    /// <param name="ai1">The first AI persona.</param>
    /// <param name="ai2">The second AI persona.</param>
    /// <param name="startingText">The initial text to start the conversation.</param>
    /// <returns>A task that represents the asynchronous initialization operation.</returns>
    public async Task Initialize(AIPersona ai1, AIPersona ai2, string startingText)
    {
        m_aiPersona = [ai1, ai2];
        m_lastOutput = new StringBuilder(startingText);
        await foreach (var _ in ai1.Chat!.SendAsAsync(ChatRole.Assistant, startingText));
        m_currentPersonaIndex = 1;
    }

    /// <summary>
    /// Generates the next output in the conversation by alternating to the next persona.
    /// </summary>
    /// <param name="outputText">An action to handle each token of the generated output.</param>
    /// <returns>A task that represents the asynchronous generation operation.</returns>
    public async Task GenerateNextOutput(Action<string> outputText)
    {
        var previousMessage = m_lastOutput.ToString();
        m_lastOutput.Clear();
        await foreach (var token in m_aiPersona[m_currentPersonaIndex].Chat!.SendAsync(previousMessage))
        {
            outputText(token);
            m_lastOutput.Append(token);
        }
        m_currentPersonaIndex = (m_currentPersonaIndex + 1) % m_aiPersona.Length;
    }
}
