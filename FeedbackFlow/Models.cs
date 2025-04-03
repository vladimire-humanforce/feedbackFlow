
namespace FeedbackFlow
{
    public record TextPromptRequest(string Prompt, LLModel Model = LLModel.Claude_3_haiku, KnowledgeBase Base = KnowledgeBase.Feedback);

    public enum LLModel
    {
        Claude_3_haiku,
        Claude_3_sonnet,
        Claude_3_5_sonnet_v1,
        Claude_3_5_sonnet_v2,
        Nova_pro_v1,
        Nova_lite_v1,
        Nova_micro_v1,
        Mistral_7b_instruct
    }

    public enum KnowledgeBase
    {
        Feedback,
        NPS
    }
}
