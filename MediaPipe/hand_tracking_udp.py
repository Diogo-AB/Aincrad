import cv2
import mediapipe as mp
import socket
import json
import math

# URL do stream do celular (IP Webcam)
url = "http://192.168.127.66:8080/video"  # substitua pelo seu IP
cap = cv2.VideoCapture(url)

# Inicializa MediaPipe Hands
mp_hands = mp.solutions.hands
mp_draw = mp.solutions.drawing_utils
hands = mp_hands.Hands(max_num_hands=1, min_detection_confidence=0.7, min_tracking_confidence=0.7)


# Configura UDP
UDP_IP = "127.0.0.1"
UDP_PORT = 5005
sock = socket.socket(socket.AF_INET, socket.SOCK_DGRAM)

# Variáveis para movimento
prev_x, prev_y = 0, 0
movement_threshold = 0.01
STOP_VALUE = "0,0,0"

def hand_open_or_closed(landmarks):
    tip_ids = [8, 12, 16, 20]
    mid_ids = [6, 10, 14, 18]

    distances = []
    for tip, mid in zip(tip_ids, mid_ids):
        dist = finger_bend_level(landmarks[tip], landmarks[mid])
        distances.append(dist)

    avg_dist = sum(distances) / len(distances)

    # Ajuste fino baseado no comportamento real da mão
    if avg_dist > 0.11:
        return "Aberta"
    elif avg_dist > 0.07:
        return "Meio"
    else:
        return "Fechada"

def finger_bend_level(lm_tip, lm_mid):
    # Distância Euclidiana entre ponta e articulação
    return math.dist(lm_tip, lm_mid)

while True:
    ret, frame = cap.read()
    if not ret:
        continue

    frame = cv2.flip(frame, 1)
    rgb_frame = cv2.cvtColor(frame, cv2.COLOR_BGR2RGB)
    results = hands.process(rgb_frame)

    if results.multi_hand_landmarks:
        for hand_landmarks in results.multi_hand_landmarks:
            mp_draw.draw_landmarks(frame, hand_landmarks, mp_hands.HAND_CONNECTIONS)
            landmarks = [[lm.x, lm.y, lm.z] for lm in hand_landmarks.landmark]
            for lm in landmarks:
                lm[0] = 1 - lm[0]

            # Estado da mão
            state = hand_open_or_closed(landmarks)

            # Movimento
            wrist_x, wrist_y = landmarks[0][0], landmarks[0][1]
            dx = wrist_x - prev_x
            dy = wrist_y - prev_y
            movement = ""
            if dx > movement_threshold: movement += "Direita "
            elif dx < -movement_threshold: movement += "Esquerda "
            if dy > movement_threshold: movement += "Baixo"
            elif dy < -movement_threshold: movement += "Cima"
            if movement == "": movement = "Parado"

            prev_x, prev_y = wrist_x, wrist_y

            # Envia UDP para Unity
            data = {"hand_state": state, "movement": movement.strip()}
            sock.sendto(json.dumps(data).encode(), (UDP_IP, UDP_PORT))

            # Debug na tela
            cv2.putText(frame, f"Mao: {state}", (10, 50), cv2.FONT_HERSHEY_SIMPLEX, 1, (0,255,0), 2)
            cv2.putText(frame, f"Mov: {movement.strip()}", (10, 100), cv2.FONT_HERSHEY_SIMPLEX, 1, (0,0,255), 2)
    else:
        data = {"hand_state": "None", "movement": "Parado"}
        sock.sendto(json.dumps(data).encode(), (UDP_IP, UDP_PORT))

    cv2.imshow("Hand Tracking", frame)
    if cv2.waitKey(1) & 0xFF == ord('q'):
        break

cap.release()
cv2.destroyAllWindows()
