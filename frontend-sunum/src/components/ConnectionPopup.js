import React, { useState } from 'react';
import { Modal, Button } from 'react';

const ConnectionPopup = ({ onSelect }) => {
    const [show, setShow] = useState(true);

    const handleClose = () => setShow(false);
    const handleSelect = (option) => {
        onSelect(option);
        handleClose();
    };

    return (
        <Modal show={show} onHide={handleClose}>
            <Modal.Header closeButton>
                <Modal.Title>Connection Type</Modal.Title>
            </Modal.Header>
            <Modal.Body>
                <p>Please select the connection type you want to use:</p>
                <Button variant="primary" onClick={() => handleSelect('StompServer')}>StompServer</Button>
                <Button variant="secondary" onClick={() => handleSelect('SignalR')}>SignalR</Button>
            </Modal.Body>
        </Modal>
    );
};

export default ConnectionPopup;
