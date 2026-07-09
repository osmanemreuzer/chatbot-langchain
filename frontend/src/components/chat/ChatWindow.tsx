import { useEffect, useRef, useState } from "react";
import { MessageCircle } from "lucide-react";
import { getConversation, sendMessage } from "../../api/chat";
import MessageBubble from "./MessageBubble";
import ChatInput from "./ChatInput";
import ChatHeader from "./ChatHeader";
import type { Message } from "../../types";

interface Props {
  conversationId: string;
  conversationTitle?: string;
}

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

  useEffect(() => {
    bottomRef.current?.scrollIntoView({ behavior: "smooth" });
  }, [messages]);

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
          <MessageCircle size={40} color="#8696a0" />
        </div>
        <p>Sol panelden bir sohbet seçin</p>
        <p style={{ fontSize: 13 }}>veya yeni sohbet başlatın</p>
      </div>
    );
  }

  return (
    <div className="chat-window">
      <ChatHeader title={conversationTitle || "Yeni Sohbet"} />
      <div className="messages-container">
        {loading && <p className="loading-text">Yükleniyor...</p>}
        {messages.map((msg) => (
          <MessageBubble key={msg.id} message={msg} />
        ))}
        {sending && (
          <div className="message-row assistant">
            <div className="message-bubble assistant typing-bubble">
              <span className="typing-dot" />
              <span className="typing-dot" />
              <span className="typing-dot" />
            </div>
          </div>
        )}
        <div ref={bottomRef} />
      </div>
      <ChatInput onSend={handleSend} disabled={sending} />
    </div>
  );
}
