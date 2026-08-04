import { Sparkles } from "lucide-react";

interface Props {
  title: string;
}

export default function ChatHeader({ title }: Props) {
  return (
    <div className="chat-header">
      <div className="chat-header-avatar">
        <Sparkles size={18} />
      </div>
      <div className="chat-header-info">
        <div className="chat-header-name">{title || "Yeni Sohbet"}</div>
      </div>
    </div>
  );
}
