import { useEffect, useRef, useState } from "react";
import { Sparkles, MessageCircle } from "lucide-react";
import { getConversation, sendMessage } from "../../api/chat";
import MessageBubble from "./MessageBubble";
import ChatInput from "./ChatInput";
import ChatHeader from "./ChatHeader";
import type { Message, SuggestedQuestion } from "../../types";

interface Props {
  conversationId: string;
  conversationTitle?: string;
}

// Doküman-özel dinamik öneri üretimi (AI servisine ekstra çağrı, token maliyeti)
// şimdilik devre dışı — sabit liste kullanılıyor. Geri açmak için:
// getSuggestedQuestions import'unu ve aşağıdaki fetch useEffect'ini geri getir.
const SUGGESTIONS: SuggestedQuestion[] = [
  {
    title: "Membran kirlenmesi",
    question: "RO membranlarında kirlenme (fouling) belirtileri nelerdir ve nasıl önlenir?",
  },
  {
    title: "Su kimyası",
    question: "Besleme suyunun RO sistemi için uygunluğunu değerlendirirken nelere bakmalıyım?",
  },
  {
    title: "Temizlik / CIP",
    question: "RO membranları için CIP (yerinde temizlik) prosedürünü adım adım anlat.",
  },
  {
    title: "Arıza giderme",
    question: "Permeat akışında ani bir düşüş yaşanıyor, olası nedenleri neler olabilir?",
  },
];

export default function ChatWindow({ conversationId, conversationTitle }: Props) {
  const [messages, setMessages] = useState<Message[]>([]);
  const [loading, setLoading] = useState(false);
  const [sending, setSending] = useState(false);
  const bottomRef = useRef<HTMLDivElement>(null);

  useEffect(() => {
    if (!conversationId) return;
    setLoading(true);
    getConversation(conversationId)
      .then((r) => setMessages(r.data.messages))
      .finally(() => setLoading(false));
  }, [conversationId]);

  const hasUserMessage = messages.some((m) => m.role === "user");
  const showWelcome = !loading && !hasUserMessage && !sending;

  useEffect(() => {
    if (hasUserMessage) {
      bottomRef.current?.scrollIntoView({ behavior: "smooth" });
    }
  }, [messages, hasUserMessage]);

  const handleSend = async (content: string) => {
    const tempId = `temp-${Date.now()}`;
    const optimisticMessage: Message = {
      id: tempId,
      role: "user",
      content,
      createdAt: new Date().toISOString(),
    };
    setMessages((prev) => [...prev, optimisticMessage]);
    setSending(true);
    try {
      const { data } = await sendMessage(conversationId, content);
      setMessages((prev) => [...prev.filter((m) => m.id !== tempId), ...data]);
    } finally {
      setSending(false);
    }
  };

  if (!conversationId) {
    return (
      <div className="chat-empty">
        <div className="chat-empty-icon">
          <MessageCircle size={28} />
        </div>
        <h2>Bugün nasıl yardımcı olabilirim?</h2>
        <p>Sol panelden bir sohbet seçin veya yeni sohbet başlatın</p>
      </div>
    );
  }

  return (
    <div className="chat-window">
      <ChatHeader title={conversationTitle || "Yeni Sohbet"} />
      <div className="messages-container">
        <div className="messages-inner">
          {loading && <p className="loading-text">Yükleniyor...</p>}
          {showWelcome && (
            <div className="conv-welcome">
              <div className="conv-welcome-icon">
                <Sparkles size={22} />
              </div>
              <h2 className="conv-welcome-title">Bugün nasıl yardımcı olabilirim?</h2>
              <p className="conv-welcome-subtitle">
                RO/NF membran sistemleri, su kimyası ve işletme konularında yardımcı olabilirim. Bir örnekle başlayabilirsin:
              </p>
              <div className="suggestion-grid">
                {SUGGESTIONS.map((s) => (
                  <button
                    key={s.title}
                    className="suggestion-card"
                    onClick={() => handleSend(s.question)}
                  >
                    <strong>{s.title}</strong>
                    <span>{s.question}</span>
                  </button>
                ))}
              </div>
            </div>
          )}
          {!showWelcome && messages.map((msg) => (
            <MessageBubble key={msg.id} message={msg} />
          ))}
          {sending && (
            <div className="message-row assistant typing-row">
              <div className="message-avatar">
                <Sparkles size={14} />
              </div>
              <div className="typing-dots">
                <span className="typing-dot" />
                <span className="typing-dot" />
                <span className="typing-dot" />
              </div>
            </div>
          )}
          <div ref={bottomRef} />
        </div>
      </div>
      <ChatInput onSend={handleSend} disabled={sending} />
    </div>
  );
}
